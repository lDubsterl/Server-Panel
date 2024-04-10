create table if not exists "Site accounts"
(
	Id serial primary key,
	Email varchar,
	Password varchar
);

create table if not exists "Ssh accounts"
(
	Id serial primary key,
	SshUsername varchar,
	SshPassword varchar,
	Minecraft bool default false,
	DST bool default false
);