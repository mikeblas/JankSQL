

CREATE TABLE MyTable (keycolumn INTEGER, city_name VARCHAR(40), state VARCHAR(40), population INTEGER);

INSERT INTO MyTable(keycolumn, city_name, state, population) VALUES
(1, 'Monroeville', 'Pennsylvania', 25000),
(2, 'Sammamish', 'Washington', 37000),
(3, 'New York', 'New York', 11500000);


SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 2


SELECT '300' + 5;
SELECT 5 + '300';


CREATE TABLE X3 (Somecol INTEGER);
TRUNCATE TABLE X3;