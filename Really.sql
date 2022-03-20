
DROP TABLE MyTable;
CREATE TABLE MyTable (keycolumn INTEGER, city_name VARCHAR(40), state VARCHAR(40), population INTEGER);

INSERT INTO MyTable(keycolumn, city_name, state, population) VALUES
(1, 'Monroeville', 'Pennsylvania', 25000),
(2, 'Sammamish', 'Washington', 37000),
(3, 'New York', 'New York', 11500000);


DROP TABLE Ten;
CREATE TABLE Ten (number_id INTEGER, number_name VARCHAR(20), is_even INTEGER);

INSERT INTO Ten (number_id, number_name, is_even) VALUES
(1, 'one',   0),
(2, 'two',   1),
(3, 'three', 0),
(4, 'four',  1),
(5, 'five',  0),
(6, 'six',   1),
(7, 'seven', 0),
(8, 'eight', 1),
(9, 'nine',  0),
(0, 'zero',  1);


DROP TABLE KeyOrdering;
CREATE TABLE KeyOrdering (K1 INTEGER NOT NULL, K2 INTEGER NOT NULL, K3 INTEGER NOT NULL, Description VARCHAR(10));

INSERT INTO KeyOrdering (K1, K2, K3, Description) VALUES
(0,0,0,'first'),
(0,1,0,'fifth'),
(0,0,1,'third'),
(0,1,1,'seventh'),
(1,0,0,'second'),
(1,1,0,'sixth'),
(1,0,1,'fourth'),
(1,1,1,'eighth');


