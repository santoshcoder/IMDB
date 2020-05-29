using IMDB.Edmx;
using IMDB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using static IMDB.Models.MovieModel;

namespace IMDB.Api
{
    [RoutePrefix("api/Movie")]
    public class MovieController : ApiController
    {
        [Route("GetGenders"), HttpGet]
        public List<GenderModel> GetGenders()
        {

            List<GenderModel> Genders = new List<GenderModel>();

            using (IMDBEntities db = new IMDBEntities())
            {
                Genders = db.Genders.Select(a => new GenderModel
                {
                    Id = a.GenderId,
                    Name = a.Name
                }).ToList();
            }

            return Genders;
        }


        [Route("GetMovies"), HttpPost]
        public dynamic GetMovies(CustomModel CustomModel)
        {

            List<MovieModel> Movies = new List<MovieModel>();
            var totalMoviesCount = 0;
            var totalMoviesCountAfterFilter = 0;
            using (IMDBEntities db = new IMDBEntities())
            {

                Movies = db.Movies.Select(a => new MovieModel
                {
                    MovieId=a.MovieId,
                    Name = a.Name,
                    ProducerName = a.ActorsAndProducer.Name,
                    Actors = a.CastInMovies.Select(k => new UserModel
                    {
                        ActorOrProducerId=k.ActorsAndProducer.ActorOrProducerId,
                        Name = k.ActorsAndProducer.Name
                    }).ToList(),
                    YearOfRelease = a.YearOfRelease,
                    Poster=a.Poster,
                    ProducerId=a.ProducerId
                }).ToList();

                if (Movies.Count() > 0)
                {
                    string search = CustomModel.CustomSearch;
                    totalMoviesCount = Movies.Count();
                    if (search != "" && search != null)
                    {

                        Movies = Movies.Where(c => (c.Name.ToLower().Contains(search.ToLower())) ||
                        (c.ProducerName != null ? (c.ProducerName.ToLower().Contains(search.ToLower())) : false) ||
                        (c.YearOfRelease.ToString().ToLower().Contains(search.ToLower())) ||
                        (c.Actors.Where(a=>a.Name.ToLower().Contains(search.ToLower())).Count() >0)
                        ).ToList();

                    }
                    totalMoviesCountAfterFilter = Movies.Count();
                    if (CustomModel.PageSortDir == "asc")
                    {

                        Movies = Movies.OrderBy(x => x.GetType().GetProperty(CustomModel.PageSortColName).GetValue(x, null))
                                    .Skip(CustomModel.PageIndex).Take(CustomModel.PageLength).ToList();
                    }
                    else
                    {
                        if (CustomModel.PageSortDir != null)
                        {
                            Movies = Movies.OrderByDescending(x => x.GetType().GetProperty(CustomModel.PageSortColName)
                                        .GetValue(x, null)).Skip(CustomModel.PageIndex).Take(CustomModel.PageLength).ToList();
                        }
                    }
                    if (CustomModel.PageSortDir == null)
                        Movies = Movies.OrderByDescending(a => a.Name).Skip(CustomModel.PageIndex).Take(CustomModel.PageLength).ToList();

                }

            }
            return new
            {

                recordsTotal = totalMoviesCount,
                recordsFiltered = totalMoviesCountAfterFilter,
                data = Movies
            };
        }

        [Route("GetActors"), HttpGet]
        public List<UserModel> GetActors()
        {

            List<UserModel> Actors = new List<UserModel>();

            using (IMDBEntities db = new IMDBEntities())
            {
                Actors = db.ActorsAndProducers.Where(a => a.IsActor == true).Select(a => new UserModel
                {
                    ActorOrProducerId = a.ActorOrProducerId,
                    Name = a.Name
                }).ToList();
            }

            return Actors;
        }

        [Route("GetProducers"), HttpGet]
        public List<UserModel> GetProducers()
        {

            List<UserModel> Actors = new List<UserModel>();

            using (IMDBEntities db = new IMDBEntities())
            {
                Actors = db.ActorsAndProducers.Where(a => a.IsActor == false).Select(a => new UserModel
                {
                    ActorOrProducerId = a.ActorOrProducerId,
                    Name = a.Name
                }).ToList();
            }

            return Actors;
        }

        [Route("GetUser"), HttpGet]
        public UserModel GetUser(int Id)
        {

            UserModel User = new UserModel();
            using (IMDBEntities db = new IMDBEntities())
            {
                User = db.ActorsAndProducers.Where(a => a.ActorOrProducerId==Id).Select(a => new UserModel
                {
                    Sex=a.Sex,
                    BIO=a.BIO,
                    ActorOrProducerId = a.ActorOrProducerId,
                    Name = a.Name,
                    DOB=a.DOB
                }).FirstOrDefault();
            }

            return User;
        }

        [Route("CreateMovie"), HttpPost]
        public dynamic CreateMovie()
        {
            MovieModel Movie = new MovieModel();
            Movy MovieDB = new Movy();
            try
            {
                using (IMDBEntities db = new IMDBEntities())
                {

                    if (HttpContext.Current.Request.Form["Movie"] != null)
                    {
                        Movie = JsonConvert.DeserializeObject<MovieModel>(HttpContext.Current.Request.Form["Movie"]);
                        if (Movie.MovieId > 0)
                        {
                            MovieDB = db.Movies.Where(a => a.MovieId == Movie.MovieId).FirstOrDefault();
                            MovieDB.CastInMovies.Clear();

                        }
                       
                       
                        MovieDB.Name = Movie.Name;
                        MovieDB.Plot = Movie.Plot;
                        MovieDB.YearOfRelease = Movie.YearOfRelease;

                        //Saving Producer
                        if (Movie.ProducerId <= 0)
                        {
                            ActorsAndProducer Producer = new ActorsAndProducer();
                            Producer.Name = Movie.ProducerName;
                            Producer.IsActor = false;
                            db.ActorsAndProducers.Add(Producer);
                            db.SaveChanges();
                            Movie.ProducerId = Producer.ActorOrProducerId; ;
                        }

                        MovieDB.ProducerId = Movie.ProducerId;

                        //Saving Actor
                        List<ActorsAndProducer> Actors = new List<ActorsAndProducer>();
                        foreach (var actor in Movie.Actors)
                        {
                            CastInMovie cast = new CastInMovie();
                            if (actor.IsNew == true)
                            {
                                ActorsAndProducer Actor = new ActorsAndProducer();
                                Actor.Name = actor.Name;
                                Actor.IsActor = true;
                                db.ActorsAndProducers.Add(Actor);
                                db.SaveChanges();
                                actor.ActorOrProducerId = Actor.ActorOrProducerId;
                            }
                            
                            cast.MovieId = MovieDB.MovieId;
                            cast.ActorId = actor.ActorOrProducerId;

                            MovieDB.CastInMovies.Add(cast);
                        }

                    }

                    if (System.Web.HttpContext.Current.Request.Files.Count > 0)
                    {
                        System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["Poster"];
                        if (httpPostedFile != null)
                        {
                            Guid Guid = Guid.NewGuid();
                            var fileSavePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Images"), (Guid + "_" + httpPostedFile.FileName));
                            MovieDB.Poster = "Images/" + Guid+"_"+ httpPostedFile.FileName;
                            httpPostedFile.SaveAs(fileSavePath);
                        }
                    }
                    else
                    {
                        MovieDB.Poster = Movie.Poster;
                    }

                    if (Movie.MovieId < 0 )
                    {
                        db.Movies.Add(MovieDB);
                    }
                    var result = db.SaveChanges();
                    if (result > 0)
                    {
                        return true;
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        [Route("SaveUser"), HttpPost]
        public bool SaveUser(UserModel User)
        {

            ActorsAndProducer s = new ActorsAndProducer();
            if (User != null)
            {              
                using (IMDBEntities db = new IMDBEntities())
                {
                    s = db.ActorsAndProducers.Where(a => a.ActorOrProducerId == User.ActorOrProducerId).FirstOrDefault();
                    if (s!= null)
                    {
                        s.Name = User.Name;
                        s.Sex = User.Sex;
                        s.DOB = User.DOB;
                        s.BIO = User.BIO;
                    }
                    var result = db.SaveChanges();
                    if (result > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;

        }


        [Route("GetMovie"), HttpGet]
        public MovieModel GetMovie(int Id)
        {

            MovieModel Movie = new MovieModel();
            using (IMDBEntities db = new IMDBEntities())
            {
                Movie = db.Movies.Where(a=>a.MovieId==Id).Select(a => new MovieModel
                {
                    MovieId=a.MovieId,
                    Name = a.Name,
                    ProducerName = a.ActorsAndProducer.Name,
                    ProducerId = a.ProducerId,
                    Actors = a.CastInMovies.Select(k => new UserModel
                    {
                        ActorOrProducerId = k.ActorsAndProducer.ActorOrProducerId,
                        Name = k.ActorsAndProducer.Name
                    }).ToList(),
                    YearOfRelease = a.YearOfRelease,
                    Poster = a.Poster,
                    Plot=a.Plot
                    
                }).FirstOrDefault();
            }

            return Movie;
        }
    }

}
public class GenderModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CustomModel
{
    public int PageLength { get; set; }
    public int PageIndex { get; set; }
    public int PageSortCol { get; set; }
    public string PageSortColName { get; set; }
    public string PageSortDir { get; set; }
    public string CustomSearch { get; set; }
    public string MobiPageSortColNamele { get; set; }
}