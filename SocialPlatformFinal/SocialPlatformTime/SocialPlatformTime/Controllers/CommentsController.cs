using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    public class CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public IActionResult Index(int id)
        {
            var comments = _db.Comments
                            .Where(c => c.PostId == id)
                            .Include(c => c.ApplicationUser)
                            .OrderByDescending(p => p.Date)
                            .ToList();  

            ViewBag.PostId = id; // îl folosim în formularul de adăugare comentariu
            ViewBag.Comments = comments;

            SetAccessRights();

            return View(comments);
        }




        //// Add a comm for an asociated post
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;
            comm.ApplicationUserId = _userManager.GetUserId(User);
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                comm.ApplicationUserId = _userManager.GetUserId(User);
                _db.Comments.Add(comm);
                _db.SaveChanges();
                return Redirect("/Posts/Show/" + comm.PostId);
            }
            else
            {
                //foreach (var modelState in ModelState.Values)
                //{
                //    foreach (var error in modelState.Errors)
                //    {
                //        Console.WriteLine("Eroare: " + error.ErrorMessage);
                //    }
                //}
                return Redirect("/Posts/Show/" + comm.PostId);
            }
        }


        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent
        // Se poate edita comentariul doar de catre utilizatorul care a postat comentariul respectiv
        // [HttpGet] se executa implicit 

        [Authorize(Roles = "Registered_User")]
        public IActionResult Edit(int id)
        {
            Comment? comm = _db.Comments.Find(id);

            if (comm is null)
            {
                return NotFound();
            }
            else
            {
                // comm.ApplicationUserId - id-ul din baza de date
                if (comm.ApplicationUserId == _userManager.GetUserId(User))
                {
                    //SetAccessRights();
                    return View(comm);
                }
                else
                {
                    TempData["message"] = "You can't edit this comment becuase it's not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts"); // primul parametru este numele actiunii, al doilea parametru ii spune unde sa mearga (routevalue)
                }
            }
        }


        [HttpPost]
        [Authorize(Roles = "Registered_User")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment? comm = _db.Comments.Find(id);

            if (comm is null)
            {
                return NotFound();
            }
            else // l-a gasit
            {
                if (comm.ApplicationUserId == _userManager.GetUserId(User))
                {
                    ModelState.Remove(nameof(Comment.ApplicationUserId));
                    ModelState.Remove(nameof(Comment.PostId));
                    ModelState.Remove(nameof(Comment.Date));
                    ModelState.Remove(nameof(Comment.Id));
                    if (ModelState.IsValid)
                    {

                        comm.CommentBody = requestComment.CommentBody;
                        comm.EditedDate = DateTime.Now; // SETĂM data editării

                        _db.SaveChanges();

                        return Redirect("/Posts/Show/" + comm.PostId);
                    }
                    else
                    {
                        // Restaurăm valorile pentru a afișa formularul corect
                        requestComment.Id = comm.Id;
                        requestComment.PostId = comm.PostId;
                        requestComment.ApplicationUserId = comm.ApplicationUserId;
                        requestComment.Date = comm.Date;
                        requestComment.EditedDate = comm.EditedDate;
                        //SetAccessRights();
                        Console.WriteLine("Nu merge");
                        return View(requestComment);
                    }
                }
                //utilizatorul nu e eligibil sa faca modificari
                else
                {
                    TempData["message"] = "You can't edit this comment becuase it's not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts"); // primul parametru este numele actiunii, al doilea parametru ii spune unde sa mearga (routevalue)
                }
            }

        }


        // Delete a comm from a post
        // Se poate sterge comentariul doar de catre utilizatorul care a postat comentariul respectiv sau de catre administrator
        [HttpPost]
        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult Delete(int id)
        {
            Comment comm = _db.Comments.Find(id);
            if (comm is null)
            {
                return NotFound();
            }
            else
            {
                if (comm.ApplicationUserId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
                {
                    _db.Comments.Remove(comm);
                    _db.SaveChanges();
                    return Redirect("/Posts/Show/" + comm.PostId);
                }
                else
                {
                    TempData["message"] = "You can't delete this comment becuase it's not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts"); // primul parametru este numele actiunii, al doilea parametru ii spune unde sa mearga (routevalue)
                }
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
