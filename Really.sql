

CREATE TABLE MyTable (keycolumn INTEGER, city_name VARCHAR(40), state VARCHAR(40), population INTEGER);

INSERT INTO MyTable(keycolumn, city_name, state, population) VALUES
(1, 'Monroeville', 'Pennsylvania', 25000),
(2, 'Sammamish', 'Washington', 37000),
(3, 'New York', 'New York', 11500000);


SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 2


SELECT '300' + 5;
SELECT 5 + '300';


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


