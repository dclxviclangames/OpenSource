CREATE TABLE players (
    player_id TEXT PRIMARY KEY,
    currency INTEGER NOT NULL DEFAULT 0,
    owned_skins TEXT NOT NULL DEFAULT '[]', -- JSON-массив
    active_skin TEXT NOT NULL DEFAULT 'default'
);
