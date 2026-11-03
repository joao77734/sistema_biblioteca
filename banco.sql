CREATE TABLE avaliacao(
    id_avaliacao int auto_increment primary key,
    id_pesquisa int not null,
    id_avaliador int not null,
    resultado varchar(200),
    justificativa text,
    data_avaliacao date,
    
    foreign key (id_pesquisa) references pesquisa(id_pesquisa),
    foreign key (id_avaliador) references usuario(id_avaliador)
);