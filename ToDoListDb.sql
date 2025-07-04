-- Таблица пользователей (соответствует классу ToDoUser)
CREATE TABLE ToDoUser (
    UserId UUID PRIMARY KEY,
    TelegramUserId BIGINT NOT NULL UNIQUE,
    TelegramUserName VARCHAR(100) NOT NULL,
    RegisteredAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Таблица списков (соответствует классу ToDoList)
CREATE TABLE ToDoList (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    UserId UUID NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_ToDoList_ToDoUser FOREIGN KEY(UserId) 
        REFERENCES ToDoUser(UserId) ON DELETE CASCADE
);

-- Таблица задач (соответствует классу ToDoItem)
CREATE TABLE ToDoItem (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    UserId UUID NOT NULL,
    ListId UUID,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    State INTEGER NOT NULL DEFAULT 0, 
    StateChangedAt TIMESTAMP NULL,
    Deadline TIMESTAMP NOT NULL,
    
    CONSTRAINT FK_ToDoItem_ToDoUser FOREIGN KEY(UserId) 
        REFERENCES ToDoUser(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_ToDoItem_ToDoList FOREIGN KEY(ListId) 
        REFERENCES ToDoList(Id) ON DELETE SET NULL
);


CREATE INDEX IX_ToDoItem_UserId ON ToDoItem(UserId);
CREATE INDEX IX_ToDoItem_ListId ON ToDoItem(ListId);
CREATE INDEX IX_ToDoList_UserId ON ToDoList(UserId);

CREATE UNIQUE INDEX UQ_ToDoUser_TelegramUserId ON ToDoUser(TelegramUserId);