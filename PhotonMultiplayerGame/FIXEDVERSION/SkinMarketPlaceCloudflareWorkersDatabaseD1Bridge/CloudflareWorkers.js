const API_AUTH_TOKEN = "your_secret_game_token_123"; 

export default {
  // Env содержит привязку D1 Database под именем 'DB'
  async fetch(request, env) {
    const url = new URL(request.url);
    const pathSegments = url.pathname.split('/').filter(segment => segment.length > 0);
    
    // --- АУТЕНТИФИКАЦИЯ ---
    if (request.headers.get("Authorization") !== `Bearer ${API_AUTH_TOKEN}`) {
        return new Response("Unauthorized", { status: 401 });
    }

    // Проверка пути /api/inventory/{action}/{playerID}
    if (pathSegments[0] !== 'api' || pathSegments[1] !== 'inventory') {
        return new Response("Not Found", { status: 404 });
    }

    const action = pathSegments[2]; 
    const playerID = pathSegments[3]; 

    if (!playerID) {
        return new Response("Player ID is required.", { status: 400 });
    }

    // --- ЛОГИКА СОХРАНЕНИЯ (POST /save/{playerID}) ---
    if (action === 'save' && request.method === 'POST') {
        try {
            const data = await request.json(); 

            // Валидация полей: coins, activeSkinId, ownedSkins
            if (data.coins === undefined || !data.activeSkinId || !Array.isArray(data.ownedSkins)) {
                 return new Response("Missing required skin/coin fields.", { status: 400 });
            }

            const dataJson = JSON.stringify(data);

            // INSERT OR REPLACE для сохранения или обновления
            const { success } = await env.DB.prepare(
                "INSERT OR REPLACE INTO player_saves (player_id, data_json) VALUES (?1, ?2)"
            )
            .bind(playerID, dataJson)
            .run();

            if (success) {
                return new Response("Save successful", { status: 200 });
            } else {
                return new Response("Save failed", { status: 500 });
            }
        } catch (e) {
            return new Response(`Error processing save request: ${e.message}`, { status: 500 });
        }
    }

    // --- ЛОГИКА ЗАГРУЗКИ (GET /load/{playerID}) ---
    if (action === 'load' && request.method === 'GET') {
        try {
            const { results } = await env.DB.prepare(
                "SELECT data_json FROM player_saves WHERE player_id = ?1"
            )
            .bind(playerID)
            .all();

            if (results.length > 0) {
                // Возвращаем сохраненный JSON
                return new Response(results[0].data_json, {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                });
            } else {
                // 404 для нового игрока (нет сохранения)
                return new Response("No save data found.", { status: 404 });
            }
        } catch (e) {
            return new Response(`Error processing load request: ${e.message}`, { status: 500 });
        }
    }

    return new Response("Invalid API Endpoint or Method", { status: 400 });
  }
};const API_AUTH_TOKEN = "your_secret_game_token_123"; 

export default {
  // Env содержит привязку D1 Database под именем 'DB'
  async fetch(request, env) {
    const url = new URL(request.url);
    const pathSegments = url.pathname.split('/').filter(segment => segment.length > 0);
    
    // --- АУТЕНТИФИКАЦИЯ ---
    if (request.headers.get("Authorization") !== `Bearer ${API_AUTH_TOKEN}`) {
        return new Response("Unauthorized", { status: 401 });
    }

    // Проверка пути /api/inventory/{action}/{playerID}
    if (pathSegments[0] !== 'api' || pathSegments[1] !== 'inventory') {
        return new Response("Not Found", { status: 404 });
    }

    const action = pathSegments[2]; 
    const playerID = pathSegments[3]; 

    if (!playerID) {
        return new Response("Player ID is required.", { status: 400 });
    }

    // --- ЛОГИКА СОХРАНЕНИЯ (POST /save/{playerID}) ---
    if (action === 'save' && request.method === 'POST') {
        try {
            const data = await request.json(); 

            // Валидация полей: coins, activeSkinId, ownedSkins
            if (data.coins === undefined || !data.activeSkinId || !Array.isArray(data.ownedSkins)) {
                 return new Response("Missing required skin/coin fields.", { status: 400 });
            }

            const dataJson = JSON.stringify(data);

            // INSERT OR REPLACE для сохранения или обновления
            const { success } = await env.DB.prepare(
                "INSERT OR REPLACE INTO player_saves (player_id, data_json) VALUES (?1, ?2)"
            )
            .bind(playerID, dataJson)
            .run();

            if (success) {
                return new Response("Save successful", { status: 200 });
            } else {
                return new Response("Save failed", { status: 500 });
            }
        } catch (e) {
            return new Response(`Error processing save request: ${e.message}`, { status: 500 });
        }
    }

    // --- ЛОГИКА ЗАГРУЗКИ (GET /load/{playerID}) ---
    if (action === 'load' && request.method === 'GET') {
        try {
            const { results } = await env.DB.prepare(
                "SELECT data_json FROM player_saves WHERE player_id = ?1"
            )
            .bind(playerID)
            .all();

            if (results.length > 0) {
                // Возвращаем сохраненный JSON
                return new Response(results[0].data_json, {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                });
            } else {
                // 404 для нового игрока (нет сохранения)
                return new Response("No save data found.", { status: 404 });
            }
        } catch (e) {
            return new Response(`Error processing load request: ${e.message}`, { status: 500 });
        }
    }

    return new Response("Invalid API Endpoint or Method", { status: 400 });
  }
};
