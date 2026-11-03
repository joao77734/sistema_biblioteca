CREATE DATABASE IF NOT EXISTS sistema_embrapa CHARACTER SET utf8mb4;

USE sistema_embrapa;

CREATE TABLE usuario (
    id_usuario int auto_increment PRIMARY KEY,
    nome varchar(120) not null,
    email varchar(150) not null unique,
    senha_hash varchar(255) not null,
    funcao varchar(100),
    status varchar(20) default 'Ativo',
    data_cadastro datetime default CURRENT_TIMESTAMP
) ENGINE=InnoDB;
