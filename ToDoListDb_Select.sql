-- 1. Получение всех задач пользователя
-- GetAllByUserId(@userId)
SELECT * FROM "ToDoItem" 
WHERE "UserId" = @userId;

-- 2. Получение активных задач пользователя ("State" = 1)
-- GetActiveByUserId(@userId)
SELECT * FROM "ToDoItem" 
WHERE "UserId" = @userId AND "State" = 1;

-- 3. Добавление новой задачи
-- Add(@item)
INSERT INTO "ToDoItem" (
    "Id", 
    "Name", 
    "UserId", 
    "ListId", 
    "CreatedAt", 
    "State", 
    "StateChangedAt", 
    "Deadline"
)
VALUES (
    @Id, 
    @Name, 
    @UserId, 
    @ListId, 
    @CreatedAt, 
    @State, 
    @StateChangedAt, 
    @Deadline
);

-- 4. Обновление задачи
-- Update(@item)
UPDATE "ToDoItem" 
SET 
    "Name" = @Name,
    "UserId" = @UserId,
    "ListId" = @ListId,
    "State" = @State,
    "StateChangedAt" = @StateChangedAt,
    "Deadline" = @Deadline
WHERE "Id" = @Id;

-- 5. Удаление задачи
-- Delete(@id)
DELETE FROM "ToDoItem" 
WHERE "Id" = @id;

-- 6. Проверка существования задачи с таким именем у пользователя
-- ExistsByName(@userId, @name)
SELECT EXISTS (
    SELECT 1 FROM "ToDoItem" 
    WHERE "UserId" = @userId AND "Name" = @name
);

-- 7. Подсчет активных задач пользователя
-- CountActive(@userId)
SELECT COUNT(*) FROM "ToDoItem" 
WHERE "UserId" = @userId AND "State" = 1;

-- 8. Поиск задач с предикатом (пример с параметрами)
-- Find(@userId, @predicate) 
SELECT * FROM "ToDoItem" 
WHERE "UserId" = @userId AND "Deadline" < @deadlineDate;

-- 9. Получение всех задач
-- GetAll()
SELECT * FROM "ToDoItem";