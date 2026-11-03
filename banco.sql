tabela tipo_acao
CREATE DATABASE IF NOT EXISTS sistema_embrapa CHARACTER SET utf8mb4;

USE sistema_embrapa;

CREATE TABLE tipo_acao (
    id_tipo_acao INT AUTO_INCREMENT PRIMARY KEY,
    nome_acao VARCHAR(120) NOT NULL,
    descricao TEXT,
    id_setor INT,

    FOREIGN KEY (id_setor)
        REFERENCES setor(id_setor)
);
