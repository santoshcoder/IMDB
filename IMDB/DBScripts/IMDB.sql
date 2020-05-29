--create table ActorsAndProducers(
--  ActorOrProducerId int primary key identity(1,1),
--  Name nvarchar(50),
--  Sex int FOREIGN KEY REFERENCES Gender(GenderId),
--  DOB int,
--  BIO nvarchar(max),
--  IsActor bit
--)

--create table Gender(
--  GenderId int primary key identity(1,1),
--  Name nvarchar(10)
--)
--create table Movies(
--  MovieId int primary key identity(1,1),
--  Name nvarchar(50),
--  YearOfRelease int,
--  Plot nvarchar(max),
--  Poster nvarchar(max),
--  ProducerId int FOREIGN KEY REFERENCES ActorsAndProducers(ActorOrProducerId)
--)

--create table CastInMovie(
--  CMID int primary key identity(1,1),
--  MovieId int FOREIGN KEY REFERENCES Movies(MovieId),
--  ActorId int FOREIGN KEY REFERENCES ActorsAndProducers(ActorOrProducerId),
  
--)


--insert into Gender values('Male')
--insert into Gender values('FeMale')





