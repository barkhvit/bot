-- 1. Заполнение таблицы ToDoUser
INSERT INTO ToDoUser (UserId, TelegramUserId, TelegramUserName, RegisteredAt)
VALUES
    ('9ccd3684-eb9a-4881-897f-c18f6efec4ae', 1976535977, 'barkhvit', '2025-01-15 10:00:00'),
	('bc2dd0ea-82dd-4f0d-b8b9-937f045d163f', 7793398569, 'vitaliyspb777', '2025-01-15 10:00:00');

-- 2. Заполнение таблицы ToDoList
INSERT INTO ToDoList (Id, Name, UserId, CreatedAt)
VALUES
    ('136f16b8-fe4f-4a28-9302-84a1654a0ceb', 'Важные', '9ccd3684-eb9a-4881-897f-c18f6efec4ae', '2025-03-01 09:15:00'),
    ('5963bef6-b17b-4c6c-8159-0d5228d61806', 'Личные', '9ccd3684-eb9a-4881-897f-c18f6efec4ae', '2025-03-05 11:20:00'),
	('136f16b8-fe4f-4a28-9302-84a1654a0c10', 'Важные', 'bc2dd0ea-82dd-4f0d-b8b9-937f045d163f', '2025-03-01 09:15:00'),
    ('5963bef6-b17b-4c6c-8159-0d5228d61807', 'Личные', 'bc2dd0ea-82dd-4f0d-b8b9-937f045d163f', '2025-03-05 11:20:00');

-- 3. Заполнение таблицы ToDoItem
INSERT INTO ToDoItem (Id, Name, UserId, ListId, CreatedAt, State, StateChangedAt, Deadline)
VALUES
    ('dfbdd492-91c6-487c-85bf-1d423287a000', 'Купить продукты', '9ccd3684-eb9a-4881-897f-c18f6efec4ae', '136f16b8-fe4f-4a28-9302-84a1654a0ceb', '2025-05-10 12:00:00', 0, NULL, '2025-05-12 18:00:00'),
    ('dfbdd492-91c6-487c-85bf-1d423287a001', 'Погулять с собакой', '9ccd3684-eb9a-4881-897f-c18f6efec4ae', '136f16b8-fe4f-4a28-9302-84a1654a0ceb', '2025-05-10 12:00:00', 0, NULL, '2025-05-12 18:00:00'),
    ('dfbdd492-91c6-487c-85bf-1d423287a002', 'Купить продукты', 'bc2dd0ea-82dd-4f0d-b8b9-937f045d163f', '136f16b8-fe4f-4a28-9302-84a1654a0c10', '2025-05-10 12:00:00', 0, NULL, '2025-05-12 18:00:00'),
    ('dfbdd492-91c6-487c-85bf-1d423287a003', 'Погулять с собакой', 'bc2dd0ea-82dd-4f0d-b8b9-937f045d163f', '136f16b8-fe4f-4a28-9302-84a1654a0c10', '2025-05-10 12:00:00', 0, NULL, '2025-05-12 18:00:00');