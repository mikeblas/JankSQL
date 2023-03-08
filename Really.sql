
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


DROP TABLE states;
CREATE TABLE states(state_code VARCHAR(20), state_name VARCHAR(20));

INSERT INTO states (state_code, state_name) VALUES
('PA', 'Pennsylvania'),
('AK', 'Arkansas'),
('HI', 'Hawaii'),
('WA', 'Washington'),
('MA', 'Massachusetts'),
('CT', 'Connecticut'),
('NY', 'New York'),
('VT', 'Vermont');



CREATE TABLE FiveLeft (number_id INTEGER, number_label VARCHAR(10));
INSERT INTO FiveLeft (number_id, number_label) VALUES
(0, 'L0'),
(1, 'L1A'),
(1, 'L1B'),
(2, 'L2'),
(3, 'L3'),
(4, 'L4'),
(5, 'L5');


CREATE TABLE FiveRight (number_id INTEGER, number_label VARCHAR(10));
INSERT INTO FiveRight (number_id, number_label) VALUES
(1, 'R1A'),
(2, 'R2'),
(3, 'R3'),
(4, 'R4A'),
(4, 'R4B'),
(5, 'R5'),
(6, 'R6');

select * from FiveLeft LEFT OUTER JOIN FiveRight ON FiveLeft.number_id = FiveRight.number_id;

select * from FiveLeft RIGHT OUTER JOIN FiveRight ON FiveLeft.number_id = FiveRight.number_id;

select FiveLeft.* from FiveLeft FULL OUTER JOIN FiveRight ON FiveLeft.number_id = FiveRight.number_id;

select * from FiveLeft FULL OUTER JOIN FiveRight ON FiveLeft.number_id = FiveRight.number_id;


