create table if not exists "Site accounts"
(
	Id serial primary key,
	Email varchar,
	Password varchar,
	Minecraft bool default false,
	DST bool default false
);