CREATE DATABASE IF NOT EXISTS sistema_embrapa CHARACTER SET utf8mb4;

USE sistema_embrapa;

CREATE TABLE setor(
    id_setor int auto_increment primary key,
    nome_setor varchar(120) not null,
    descricao varchar(255)
);

CREATE TABLE usuario(
    id_usuario int auto_increment PRIMARY KEY,
    matricula varchar(50) not null unique,
    nome varchar(150) not null,
    email varchar(150) not null unique,
    senha varchar(255) null,
    status varchar(20) not null default 'Ativo',
    id_setor int,
    foreign key (id_setor) references setor(id_setor)
);

CREATE TABLE perfil(
    id_perfil int auto_increment primary key,
    nome_perfil varchar(100) not null unique,
    descricao text
);

CREATE TABLE usuario_perfil(
    id_usuario int,
    id_perfil int,
    data_atribuicao datetime default current_timestamp,
    primary key (id_usuario, id_perfil),
    foreign key (id_usuario) references usuario(id_usuario) on delete cascade,
    foreign key (id_perfil) references perfil(id_perfil) on delete cascade
);

CREATE TABLE pesquisador(
    id_pesquisador int auto_increment primary key,
    id_usuario int not null unique,
    foreign key (id_usuario) references usuario(id_usuario) on delete cascade
);

CREATE TABLE tipo_acao(
    id_tipo_acao int auto_increment primary key,
    nome_acao varchar(120) not null,
    descricao text,
    id_setor int,
    foreign key (id_setor) references setor(id_setor)
);

CREATE TABLE ano(
    id_ano INT AUTO_INCREMENT PRIMARY KEY,
    ano int not null unique,
    abertura_fechamento varchar(50),
    status varchar(20) not null
);

CREATE TABLE acoes(
    id_acao int auto_increment primary key,
    titulo varchar(200) not null,
    descricao text not null,
    data_submissao datetime not null,
    data_atualizacao datetime null,
    status varchar(20) not null,
    id_usuario int not null,
    id_ano_tipo_acao int not null,
    foreign key (id_usuario) references usuario(id_usuario)
);

CREATE TABLE avaliacao(
    id_avaliacao int auto_increment primary key,
    id_pesquisa int not null,
    id_avaliador int not null,
    resultado varchar(200),
    justificativa text,
    data_avaliacao date,
    foreign key (id_pesquisa) references acoes(id_acao),
    foreign key (id_avaliador) references usuario(id_usuario)
);

