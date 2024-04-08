create table if not exists "Site accounts"
(
	Id serial primary key,
	Email varchar,
	Password varchar
);

create table if not exists "Ssh accounts"
(
	Id serial primary key,
	Email varchar,
	SshUsername varchar,
	SshPassword varchar
);