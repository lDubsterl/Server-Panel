create table if not exists "Site accounts"
(
	Email varchar,
	Password varchar,
	constraint "email1_pkey" primary key(Email)
);

create table if not exists "Ssh accounts"
(
	Email varchar,
	SshUsername varchar,
	SshPassword varchar,
	constraint "email2_pkey" primary key(Email)
);