using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;


        // Se afiseaza lista tuturor postarilor 
        // Pentru fiecare postare se afiseaza si numele utilizatorului care a creat-o
        //[HttpGet] care se executa implicit
        [AllowAnonymous]
        public IActionResult Index()
        {
            var posts = _db.Posts
                           .Include(p => p.ApplicationUser)
                           .OrderByDescending(p => p.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }



        // Se afiseaza o singura postare in functie de id-ul ei
        // Se preiau comentariile si reactiile asociate postarii + userul care a creat postarea
        // [HttpGet] care se executa implicit
        [AllowAnonymous]
        public IActionResult Show(int id)
        {
            Post? post = _db.Posts
                            .Include(p => p.Comments)
                                .ThenInclude(c => c.ApplicationUser) // Userul care a creat comentariul
                            .Include(p => p.Reactions)
                            .Include(p => p.ApplicationUser) // Userul care a creat postarea
                            .FirstOrDefault(p => p.Id == id);
            if (post is null)
            {
                return NotFound();
            }

            SetAccessRights();

            return View(post);
        }



        // Afiseaza formularul pentru adaugarea unei noi postari
        // [HttpGet] care se executa implicit
        [Authorize(Roles = "Utilizator_înregistrat,Administrator")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }


        [HttpPost]
        [Authorize(Roles = "Utilizator_înregistrat,Administrator")]
        public IActionResult New(Post post)
        {
            post.Date = DateTime.Now;

            post.ApplicationUserId = _userManager.GetUserId(User);


            // Validare custom: cel putin un camp trebuie completat
            if (string.IsNullOrWhiteSpace(post.PostDescription) &&
                string.IsNullOrWhiteSpace(post.TextContent) &&
                string.IsNullOrWhiteSpace(post.Image) &&
                string.IsNullOrWhiteSpace(post.Video))
            {
                ModelState.AddModelError("", "You have to complete at least one field!");
                return View(post);
            }

            //Console.WriteLine(post.ApplicationUserId);

            // ELIMINĂM eroarea pentru ApplicationUserId din ModelState
            // pentru că o setăm manual și nu vine din formular
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)                                                            
            {
                _db.Posts.Add(post);
                _db.SaveChanges();
                TempData["message"] = "The post has been added!";
                TempData["messageType"] = "alert-success";
                Console.WriteLine(post.ApplicationUserId);
                return RedirectToAction("Index");
            }

            else
            {
                return View(post);
            }
        }


        // Se editeaza un articol existent in baza de date
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        //  Doar utilizatorul care a creat postarea o poate edita
        // [HttpGet] se executa implicit

        [Authorize(Roles = "Utilizator_înregistrat")]
        public IActionResult Edit(int id) // primim id-ul articolului pe care vreau sa il editez
        {

            Post? post = _db.Posts
                            .Where(art => art.Id == id)
                            .FirstOrDefault();

            if (post is null)
            {
                return NotFound();
            }


            if (post.ApplicationUserId == _userManager.GetUserId(User)) //verific daca user-ul care l-a postat sau e admin
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "You can't edit a post that it's not yours!";
                TempData["messageType"] = "alert-danger"; // o sa fie cu rosu
                return RedirectToAction("Index");
            }

        }


        // Se editeaza o postare din BD
        //Se verifica rolul utilizatorului care are dreptul sa editeze postarea
        // Se afiseaza formularul de editare a postarii impreuna cu datele existente in BD pentru aceasta
        [HttpPost]
        [Authorize(Roles = "Utilizator_înregistrat")]
        public IActionResult Edit(int id, Post requestPost)
        {
            Post? post = _db.Posts.Find(id);

            if (post is null)
            {
                return NotFound();
            }

            else
            {
                if (ModelState.IsValid)
                {
                    if (post.ApplicationUserId == _userManager.GetUserId(User)) // utilizatorul care incearca sa modifice postarea este cel care a creat-o
                    {
                        post.PostDescription = requestPost.PostDescription;
                        post.TextContent = requestPost.TextContent;
                        post.Image = requestPost.Image;
                        post.Video = requestPost.Video;
                        post.Date = DateTime.Now;
                        TempData["message"] = "The post was modified!";
                        TempData["messageType"] = "alert-success";
                        _db.SaveChanges(); //aici se excuta query-ul
                        return RedirectToAction("Index");
                    }
                    else 
                    {

                        TempData["message"] = "You can't edit a post that it's not yours!";
                        TempData["messageType"] = "alert-danger"; // o sa fie cu rosu
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestPost); // pun requestPost ca sa pastrez datele introduse de user, nu valoarea din BD
                }
            }
        }



        // Se sterge o postare existenta in BD
        // Doar utilizatorul care a creat postarea sau administratorul o poate sterge
        //Utilizatorul poate sterge doar postarea lui
        //Administratorul poate sterge orice postare

        [HttpPost]
        [Authorize(Roles = "Utilizator_înregistrat,Administrator")]
        public ActionResult Delete(int id)
        {
            Post? post = _db.Posts.Find(id);

            if (post is null)
            {
                return NotFound();
            }
            else
            {
                // Verificam daca utilizatorul curent este cel care a creat postarea sau este admin
                if (post.ApplicationUserId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
                {
                    _db.Posts.Remove(post);
                    _db.SaveChanges();
                    TempData["message"] = "The post was deleted!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You can't delete a post that it's not yours!";
                    TempData["messageType"] = "alert-danger"; // o sa fie cu rosu
                    return RedirectToAction("Index");
                }
            }
        }

        // Add a comm for an asociated post
        // add a comment = write operation => we use HttpPost
        [HttpPost]
        [Authorize(Roles = "Utilizator_înregistrat,Administrator")]
        public IActionResult Show([FromForm] Comment comm)
        {
            comm.Date = DateTime.Now;


            // preluam id-ul userului care posteaza comentariul
            comm.ApplicationUserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                _db.Comments.Add(comm);
                _db.SaveChanges();
                return Redirect("/Posts/Show/" + comm.PostId);
            }
            else
            {
                Post? post = _db.Posts
                                .Include(p => p.Comments)
                                    .ThenInclude(c => c.ApplicationUser) // Userul care a creat comentariul
                                .Include(p => p.Reactions)
                                .Include(p => p.ApplicationUser) // Userul care a creat postarea
                                .Where(p => p.Id == comm.PostId)
                                .FirstOrDefault();
                if (post is null)
                {
                    return NotFound();
                }

                //Redirect("/Posts/Show/" + comm.PostId); // imi pierd starea modelului daca fac redirect

                SetAccessRights();

                return View(post);
            }
        }

        private void SetAccessRights()
        {
            // Luăm ID-ul celui de la tastatură
            ViewBag.UserCurent = _userManager.GetUserId(User);

            // Verificăm dacă este Admin
            ViewBag.EsteAdmin = User.IsInRole("Administrator");

            // Un flag general pentru a ști dacă userul este măcar logat
            ViewBag.EsteLogat = User.Identity.IsAuthenticated;
        }
    }
}
