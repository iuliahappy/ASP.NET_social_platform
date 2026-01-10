using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;
using SocialPlatformTime.Services;

namespace Social_Platform.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ISentimentAnalysisService sentimentService, IContentModerationService contentModerationService, ILogger<PostsController> logger) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ISentimentAnalysisService _sentimentService = sentimentService;
        private readonly IContentModerationService _contentModerationService = contentModerationService;
        private readonly ILogger<PostsController> _logger = logger;


        // Se afiseaza lista tuturor postarilor 
        // Pentru fiecare postare se afiseaza si numele utilizatorului care a creat-o
        //[HttpGet] care se executa implicit
        [AllowAnonymous]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Administrator");

            IQueryable<Post> postsQuery = _db.Posts
                .Include(p => p.ApplicationUser)
                .Include(p => p.Reactions)
                .AsQueryable();

            // Retrieve posts from those users w private profiles that have accepted the current user's fr (only if it's not an admin user)
            if (User.Identity?.IsAuthenticated == true && !isAdmin)
            {
                // retrieve list of follow reqs which have been accepted
                List<string> acceptedFollowIds = new List<string>();
                acceptedFollowIds = _db.FollowRequests
                    .Where(fr => fr.FollowerId == currentUserId && fr.Status == "accepted")
                    .Select(fr => fr.FollowingId)
                    .ToList();

                var savedPostIds = _db.SavedPosts
                    .Where(sp => sp.ApplicationUserId == currentUserId)
                    .Select(sp => sp.PostId)
                    .ToList();
                ViewBag.SavedPostIds = savedPostIds;

                postsQuery = postsQuery
                    .Where(p => p.ApplicationUser.IsPublic
                             || p.ApplicationUserId == currentUserId
                             || acceptedFollowIds.Contains(p.ApplicationUserId));
            }
            var posts = postsQuery
                .OrderByDescending(p => p.Date)
                .ToList();

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Posts = posts;


            // Obține lista de postari vizibile pentru utilizatorul curent
            if (User.Identity?.IsAuthenticated == true && !isAdmin)
            {             
                var savedPostIds = _db.SavedPosts
                    .Where(sp => sp.ApplicationUserId == currentUserId)
                    .Select(sp => sp.PostId)
                    .ToList();
                ViewBag.SavedPostIds = savedPostIds;
            }
            else
            {
                ViewBag.SavedPostIds = new List<int>(); // IMPORTANT: setează lista goală
            }

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

            //// Verifică dacă utilizatorul curent a salvat această postare
            //if (User.Identity?.IsAuthenticated == true)
            //{
            //    var currentUserId = _userManager.GetUserId(User);
            //    var isSaved = _db.SavedPosts
            //        .Any(sp => sp.PostId == id && sp.ApplicationUserId == currentUserId);
            //    ViewBag.IsSaved = isSaved;
            //}

            // Setează SavedPostIds pentru utilizatorul curent
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUserId = _userManager.GetUserId(User);
                var savedPostIds = _db.SavedPosts
                    .Where(sp => sp.ApplicationUserId == currentUserId)
                    .Select(sp => sp.PostId)
                    .ToList();
                ViewBag.SavedPostIds = savedPostIds;
            }
            else
            {
                ViewBag.SavedPostIds = new List<int>();
            }

            return View(post);
        }

        // Afiseaza formularul pentru adaugarea unei noi postari
        // [HttpGet] care se executa implicit
        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        //nou
        [HttpPost]
        [Authorize(Roles = "Registered_User,Administrator")]
        public async Task<IActionResult> NewAsync(Post post)
        {
            post.Date = DateTime.Now;
            post.ApplicationUserId = _userManager.GetUserId(User);
            ModelState.Remove("ApplicationUserId");

            // Validare custom: cel putin un camp trebuie completat
            if (string.IsNullOrWhiteSpace(post.PostDescription) &&
                string.IsNullOrWhiteSpace(post.TextContent) &&
                post.ImageFile == null &&
                post.VideoFile == null)
            {
                ModelState.AddModelError("", "You have to complete at least one field!");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                // Verificăm conținutul text pentru limbaj nepotrivit
                string textToCheck = string.Empty;
                if (!string.IsNullOrWhiteSpace(post.PostDescription))
                    textToCheck += post.PostDescription + " ";
                if (!string.IsNullOrWhiteSpace(post.TextContent))
                    textToCheck += post.TextContent;

                if (!string.IsNullOrWhiteSpace(textToCheck))
                {
                    var moderationResult = await _contentModerationService.ModerateContentAsync(textToCheck.Trim());

                    if (!moderationResult.Success)
                    {
                        // Dacă serviciul de moderare nu funcționează, logăm eroarea dar permitem postarea
                        _logger.LogWarning("Eroare la moderarea conținutului: {Error}", moderationResult.ErrorMessage);
                    }
                    else if (!moderationResult.IsAppropriate)
                    {
                        // Conținutul este neadecvat - blocăm publicarea
                        ModelState.AddModelError("", "Your content contains inappropriate language. Please revise!");
                        return View(post);
                    }
                }

                // imagine
                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await post.ImageFile.CopyToAsync(stream);
                    }

                    post.Image = "/images/" + fileName;
                }

                // video
                if (post.VideoFile != null && post.VideoFile.Length > 0)
                {
                    var videosFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
                    Directory.CreateDirectory(videosFolder);

                    var videoFileName = Guid.NewGuid().ToString() + Path.GetExtension(post.VideoFile.FileName);
                    var videoPath = Path.Combine(videosFolder, videoFileName);

                    using (var stream = new FileStream(videoPath, FileMode.Create))
                    {
                        await post.VideoFile.CopyToAsync(stream);
                    }

                    post.Video = "/videos/" + videoFileName;
                }

                _db.Posts.Add(post);
                _db.SaveChanges();
                TempData["message"] = "The post has been added!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(post);
            }
        }
        //nou

        // Se editeaza un articol existent in baza de date
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        //  Doar utilizatorul care a creat postarea o poate edita
        // [HttpGet] se executa implicit

        [Authorize(Roles = "Registered_User")]
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
        [Authorize(Roles = "Registered_User")]
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
        [Authorize(Roles = "Registered_User,Administrator")]
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
        //nou
        [HttpPost]
        [Authorize(Roles = "Registered_User,Administrator")]
        public async Task<IActionResult> Show([FromForm] Comment comm)
        {
            comm.Date = DateTime.Now;
            comm.ApplicationUserId = _userManager.GetUserId(User);
            ModelState.Remove(nameof(Comment.ApplicationUserId));

            if (ModelState.IsValid)
            {
                // Verificăm conținutul comentariului pentru limbaj nepotrivit
                if (!string.IsNullOrWhiteSpace(comm.CommentBody))
                {
                    var moderationResult = await _contentModerationService.ModerateContentAsync(comm.CommentBody);

                    if (!moderationResult.Success)
                    {
                        _logger.LogWarning("Eroare la moderarea comentariului: {Error}", moderationResult.ErrorMessage);
                    }
                    else if (!moderationResult.IsAppropriate)
                    {
                        // Comentariul este neadecvat - blocăm publicarea
                        ModelState.AddModelError("CommentBody", "Your content contains inappropriate language. Please revise!");

                        // Reîncărcăm postarea pentru a afișa eroarea
                        Post? post = _db.Posts
                            .Include(p => p.Comments)
                                .ThenInclude(c => c.ApplicationUser)
                            .Include(p => p.Reactions)
                            .Include(p => p.ApplicationUser)
                            .Where(p => p.Id == comm.PostId)
                            .FirstOrDefault();

                        if (post is null)
                        {
                            return NotFound();
                        }

                        SetAccessRights();
                        return View(post);
                    }
                }

                // Dacă conținutul este adecvat, continuăm cu analiza de sentiment
                var sentimentResult = await _sentimentService.AnalyzeSentimentAsync(comm.CommentBody);

                if (sentimentResult.Success)
                {
                    comm.SentimentLabel = sentimentResult.Label;
                    comm.SentimentConfidence = sentimentResult.Confidence;
                    comm.SentimentAnalyzedAt = DateTime.Now;
                }

                _db.Comments.Add(comm);
                _db.SaveChanges();
                return Redirect("/Posts/Show/" + comm.PostId);
            }
            else
            {
                Post? post = _db.Posts
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.ApplicationUser)
                    .Include(p => p.Reactions)
                    .Include(p => p.ApplicationUser)
                    .Where(p => p.Id == comm.PostId)
                    .FirstOrDefault();
                if (post is null)
                {
                    return NotFound();
                }

                SetAccessRights();
                return View(post);
            }
        }

        //nou

        private void SetAccessRights()
        {
            // Luăm ID-ul celui de la tastatură
            ViewBag.UserCurent = _userManager.GetUserId(User);

            // Verificăm dacă este Admin
            ViewBag.EsteAdmin = User.IsInRole("Administrator");

            // Un flag general pentru a ști dacă userul este măcar logat
            ViewBag.EsteLogat = User.Identity.IsAuthenticated;
        }

        [Authorize]
        public IActionResult Feed()
        {
            var currUserId = _userManager.GetUserId(User);

            // Retrieve Followed User Ids
            var followedIds = _db.FollowRequests
                .Where(fr => fr.FollowerId == currUserId && fr.Status == "accepted")
                .Select(fr => fr.FollowingId)
                .ToList();

            // Retrieve Posts (and associated reactions / comments) from followed Users (and current user) sorted by descending date
            var posts = _db.Posts
                .Where(p => followedIds.Contains(p.ApplicationUserId) || p.ApplicationUserId == currUserId)
                .Include(p => p.ApplicationUser)
                .Include(p => p.Reactions)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Date)
                .ToList();

            // Setează SavedPostIds pentru utilizatorul curent
            if (User.Identity?.IsAuthenticated == true && currUserId != null)
            {
                var savedPostIds = _db.SavedPosts
                    .Where(sp => sp.ApplicationUserId == currUserId)
                    .Select(sp => sp.PostId)
                    .ToList();
                ViewBag.SavedPostIds = savedPostIds;
            }
            else
            {
                ViewBag.SavedPostIds = new List<int>();
            }

            return View(posts);
        }
    }
}