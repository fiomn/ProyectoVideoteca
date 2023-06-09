﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProyectoVideoteca.Data;
using ProyectoVideoteca.Models;
using ProyectoVideoteca.Models.Domain;
using ProyectoVideoteca.Models.DTO;
using ProyectoVideoteca.Repositories.Abstract;
using QuestPDF.Helpers;
using System;
using System.Diagnostics;
using static QuestPDF.Helpers.Colors;

namespace ProyectoVideoteca.Controllers
{

    [Authorize] //everyone can use this controller
    public class KidsController : Controller
    {
        private TestUCRContext db = new TestUCRContext(); //database context
        private readonly IUserAuthenticationService _service; //database context authentication
        private readonly UserManager<ApplicationUser> _userManager;

        public KidsController(IUserAuthenticationService service, UserManager<ApplicationUser> userManager)
        {
            this._service = service;
            this._userManager = userManager;
        }

        //*************************** SERIES AND MOVIES *********************************
        public ActionResult kidsMode()
        {
            //Random random = new Random();

            var movies = new List<tb_MOVIE>();

            movies = db.tb_MOVIE.FromSqlRaw(@"exec dbo.GetMovies").ToList();

            var genres = new List<tb_GENRE>();
            genres = db.tb_GENRE.FromSqlRaw(@"exec dbo.GetGenres").ToList();

            //List<tb_GENRE> randomGenres = genres.OrderBy(x => random.Next()).ToList();

            tb_MOVIESANDGENRES moviesAndGenres = new tb_MOVIESANDGENRES(movies, genres);
            MoviesList.list = movies;
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return View();
        }

        public tb_GLOBALSETTING getMode()
        {
            //get color mode from BD
            var mode = new tb_GLOBALSETTING();
            mode = db.tb_GLOBALSETTING.FromSqlRaw("exec dbo.getMode").AsEnumerable().FirstOrDefault();
            return mode;
        }

        public ActionResult displayKidsSeries()
        {

            Random random = new Random();

            var series = new List<tb_SERIE>();

            series = db.tb_SERIE.FromSqlRaw(@"exec dbo.GetSeries").ToList();

            var genres = new List<tb_GENRE>();
            genres = db.tb_GENRE.FromSqlRaw(@"exec dbo.GetGenres").ToList();

            List<tb_GENRE> randomGenres = genres.OrderBy(x => random.Next()).ToList();

            tb_SERIESANDGENRES seriesAndGenres = new tb_SERIESANDGENRES(series, randomGenres);

            //Mode Visual Types
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;
            return View(seriesAndGenres);
        }

        //When User clicks a movie
        public ActionResult detailsMovies(string TITLE, int? page)
        {
            tb_MOVIE.currentMovie = TITLE;

            var movie = db.tb_MOVIE.FromSqlRaw(@"exec DetailsMovie @TITLE", new SqlParameter("@TITLE", tb_MOVIE.currentMovie)).ToList().FirstOrDefault();

            var comments = db.tb_RATING.FromSqlRaw(@"exec GetComments @Title", new SqlParameter("@Title", tb_MOVIE.currentMovie)).ToList();

            int itemsPerPage = 10;
            int totalPages = (int)Math.Ceiling((double)comments.Count / 10);
            totalPages = Math.Max(totalPages, 1); // Asegura que haya al menos 1 página

            int currentPage = page ?? 1; // Si no se proporciona el parámetro "page", asume la página 1

            tb_MOVIEANDCOMMENTS movieAndComments = new tb_MOVIEANDCOMMENTS(movie, comments, itemsPerPage, totalPages, currentPage);
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return View("detailsMovies", movieAndComments);
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return user;
        }


        [HttpPost]
        public IActionResult userComment(string comment, string score)
        {
            //get the current user
            var user = GetCurrentUserAsync().Result;

            float scoreF = float.Parse(score);
            scoreF = (float)Math.Round(scoreF, 2);

            db.Database.ExecuteSqlRaw(@"exec InsertCommentMovie @Title, @Username, @Comment, @Rating",
                new SqlParameter("@Title", tb_MOVIE.currentMovie),
                new SqlParameter("@Username", user.UserName),
                new SqlParameter("@Comment", comment),
                new SqlParameter("@Rating", scoreF));

            db.SaveChanges();
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return RedirectToAction("detailsMovies", "Client", new { TITLE = tb_MOVIE.currentMovie });

        }


        public ActionResult detailsSerie(string TITLE, int? page)
        {
            tb_SERIE.currentSerie = TITLE;

            var serie = db.tb_SERIE.FromSqlRaw(@"exec DetailsSeries @TITLE", new SqlParameter("@TITLE", tb_SERIE.currentSerie)).ToList().FirstOrDefault();

            var comments = db.tb_RATING.FromSqlRaw(@"exec GetComments @Title", new SqlParameter("@Title", tb_SERIE.currentSerie)).ToList();

            int itemsPerPage = 10;
            int totalPages = (int)Math.Ceiling((double)comments.Count / 10);
            totalPages = Math.Max(totalPages, 1); // Asegura que haya al menos 1 página

            int currentPage = page ?? 1; // Si no se proporciona el parámetro "page", asume la página 1

            int startIndex = (currentPage - 1) * itemsPerPage;
            int endIndex = Math.Min(startIndex + itemsPerPage, comments.Count);
            var commentsPerPage = comments.GetRange(startIndex, endIndex - startIndex);

            tb_SERIEANDCOMMENTS serieAndComments = new tb_SERIEANDCOMMENTS(serie, comments, itemsPerPage, totalPages, currentPage);
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return View("detailsSerie", serieAndComments);
        }

        public ActionResult getSeasonSerie(string selectedSeason, int? page)
        {
            var serie = db.tb_SERIE.FromSqlRaw(@"exec DetailsSeries @TITLE", new SqlParameter("@TITLE", tb_SERIE.currentSerie)).ToList().FirstOrDefault();

            var season = db.tb_SEASON.FromSqlRaw(@"exec GetSerieSeasons @TITLE, @NUMBER",
                new SqlParameter("@TITLE", serie.TITLE),
                new SqlParameter("@NUMBER", int.Parse(selectedSeason))).ToList().FirstOrDefault();

            var episodes = db.tb_EPISODE.FromSqlRaw(@"exec GetEpisodesSeason @SEASON_ID", new SqlParameter("@SEASON_ID", season.SEASON_ID)).ToList();

            var comments = db.tb_RATING.FromSqlRaw(@"exec GetComments @Title", new SqlParameter("@Title", tb_SERIE.currentSerie)).ToList();

            int itemsPerPage = 10;
            int totalPages = (int)Math.Ceiling((double)comments.Count / 10);
            totalPages = Math.Max(totalPages, 1); // Asegura que haya al menos 1 página

            int currentPage = page ?? 1; // Si no se proporciona el parámetro "page", asume la página 1

            int startIndex = (currentPage - 1) * itemsPerPage;
            int endIndex = Math.Min(startIndex + itemsPerPage, comments.Count);
            var commentsPerPage = comments.GetRange(startIndex, endIndex - startIndex);

            tb_SERIEANDCOMMENTS serieAndComments = new tb_SERIEANDCOMMENTS(serie, season, episodes, comments, itemsPerPage, totalPages, currentPage);
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return View("detailsSerie", serieAndComments);
        }

        [HttpPost]
        public IActionResult userCommentSerie(string comment, string score)
        {
            //get the current user
            var user = GetCurrentUserAsync().Result;

            float scoreF = float.Parse(score);
            scoreF = (float)Math.Round(scoreF, 2);

            db.Database.ExecuteSqlRaw(@"exec InsertCommentSerie @Title, @Username, @Comment, @Rating",
                new SqlParameter("@Title", tb_SERIE.currentSerie),
                new SqlParameter("@Username", user.UserName),
                new SqlParameter("@Comment", comment),
                new SqlParameter("@Rating", scoreF));

            db.SaveChanges();
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;

            return RedirectToAction("detailsSerie", "Client", new { TITLE = tb_SERIE.currentSerie });

        }

        //search movies by name and genre with an APi
        [HttpGet]
        public string search(string inputSearch)
        {
            var movies = new List<tb_MOVIE>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7181/Search/movies/inputSearch");
                var responseTask = client.GetAsync(inputSearch);
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<List<tb_MOVIE>>();
                    readTask.Wait();

                    movies = readTask.Result;
                    return JsonConvert.SerializeObject(movies);
                }
                return JsonConvert.SerializeObject(movies);
            }
        }

        //search series by name and genre with an APi
        [HttpGet]
        public string searchSeries(string busSeries)
        {
            var series = new List<tb_SERIE>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7181/Search/series/busSeries");
                var responseTask = client.GetAsync(busSeries);
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<List<tb_SERIE>>();
                    readTask.Wait();

                    series = readTask.Result;
                    return JsonConvert.SerializeObject(series);
                }
                return JsonConvert.SerializeObject(series);
            }
        }


        //****************************** PROFILE USER **************************
        public async Task<ActionResult> editProfile()
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
                string username = user.UserName;
                var userByName = new tb_USER();
                var parameter = new SqlParameter("@username", username);

                userByName = db.tb_USER.FromSqlRaw(@"exec getUserByName @username", new SqlParameter("@username", username)).AsEnumerable().FirstOrDefault();
                
                tb_GLOBALSETTING mode = getMode();
                ViewBag.Mode = mode.mode;
                ViewBag.ModeBtn = mode.modeBtn;
                return View(userByName);
            }
            catch (Exception ex)
            {
                tb_GLOBALSETTING mode = getMode();
                ViewBag.Mode = mode.mode;
                ViewBag.ModeBtn = mode.modeBtn;
                return View();
            }
        }

        public ActionResult edit(string userName)
        {
            var user = db.tb_USER.Find(userName);
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> edit(tb_USER user, RegistrationModel model)
        {
            try
            {
                var result = await _service.EditAsync(model); //save in context auth
                db.tb_USER.Update(user); //save users in testUCR
                db.SaveChanges();
                return RedirectToAction(nameof(editProfile));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            // Obtén el usuario actual
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                // Verifica si se seleccionó un archivo
                if (file != null && file.Length > 0)
                {
                    // Convierte el archivo en una cadena base64
                    string profilePictureBase64;
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        profilePictureBase64 = Convert.ToBase64String(fileBytes);
                    }

                    // Actualiza la imagen de perfil en el modelo del usuario
                    var userBD = db.tb_USER.FromSqlRaw(@"exec getUserByName @username", new SqlParameter("@username", user.UserName)).AsEnumerable().FirstOrDefault();
                    userBD.IMG = profilePictureBase64;
                    user.ProfilePicture = profilePictureBase64;

                    // Guarda los cambios en la base de datos
                    var result = await _userManager.UpdateAsync(user);
                    db.tb_USER.Update(userBD);
                    db.SaveChanges();
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("ChangeProfilePicture");
                    }
                }
            }
            return RedirectToAction(nameof(edit));
        }

        public async Task<IActionResult> ChangeProfilePicture()
        {
            // Obtener el usuario actualmente autenticado
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                string profilePictureBase64 = user.ProfilePicture;
                // Verificar si la imagen de perfil existe
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    // Convertir la cadena base64 en una URL válida
                    string imageFormat = "image/png"; // Cambia esto según el formato de imagen que estés utilizando
                    string profilePictureUrl = $"data:{imageFormat};base64,{profilePictureBase64}";
                    // Pasar la URL de la imagen de perfil a la vista
                    ViewBag.ProfilePicture = profilePictureUrl;
                }
                else
                {
                    ViewBag.ProfilePicture = "https://www.kindpng.com/picc/m/24-248253_user-profile-default-image-png-clipart-png-download.png";
                }
            }
            tb_GLOBALSETTING mode = getMode();
            ViewBag.Mode = mode.mode;
            ViewBag.ModeBtn = mode.modeBtn;
            return View();
        }
    }
}
