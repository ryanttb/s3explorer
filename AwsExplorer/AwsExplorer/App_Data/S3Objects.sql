-- S3Objects
create table S3Objects (
  [Key] varchar(800) not null,
  LastModified varchar(256),
  Size int,
  ContentType varchar(64),
  Content varchar(max),
  constraint PK_S3Objects primary key clustered ( [Key] asc ) with ( pad_index = off, ignore_dup_key = off ) on [PRIMARY]
) on [PRIMARY]
go

