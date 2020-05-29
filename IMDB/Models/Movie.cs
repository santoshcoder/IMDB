using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IMDB.Models
{
    public class MovieModel
    {
        public int MovieId { get; set; }
        public string Name { get; set; }
        public Nullable<int> YearOfRelease { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }
        public Nullable<int> ProducerId { get; set; }
        public string ProducerName { get; set; }
        public List<UserModel> Actors
        {
            get; set;
        }
    }

        public class UserModel
        {
            public int ActorOrProducerId { get; set; }
            public string Name { get; set; }
            public Nullable<int> Sex { get; set; }
            public Nullable<DateTime> DOB { get; set; }
            public string BIO { get; set; }
            public Nullable<bool> IsActor { get; set; }
            public Nullable<bool> IsNew { get; set; }
    }
    }