import { Hono } from "hono";
import { cors } from "hono/cors";

const app = new Hono();

app.use("/*", cors());

app.get("/start", (c) => {
    return c.text("")
})

app.post("/shutdown", (c) => {
    return c.text("")
})

app.post("/setparams", (c) => {
    return c.text("")
})

app.get("/temp", (c) => {
    let temp_min: number[] = [];
    let temp_max: number[] = [];
    let temperatures: number[] = [];
    let timestamp: number[] = [];
    const N = 15;
    let currentTime = Date.now();
    for (let i = 0; i < N; i++) {
        let min = i / (N / 10) + 12;
        let max = i / (N / 10) + 15;
        temp_min.push(min);
        temp_max.push(max);
        // Generar un número aleatorio entre -5 y 5
        let randomAdjustment = Math.random() * 10 - 5;
        temperatures.push(min + Math.random() * (max - min) + randomAdjustment);
        timestamp.push(currentTime);
        // Incrementar el tiempo actual por un valor aleatorio entre 0.1 segundos (100ms) y 1 segundo (1000ms)
        currentTime += Math.random() * (1000 - 100) + 100;
    }
    return c.json({ temp_min, temp_max, temperatures, timestamp });
});

export default app;
