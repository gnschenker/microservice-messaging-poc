CREATE TABLE cc_authentications(
   id serial PRIMARY KEY,
   datetime timestamp not null default CURRENT_TIMESTAMP,
   provider varchar(20) not null,
   cc_number varchar(20) not null,
   status VARCHAR (10) NOT NULL
);

