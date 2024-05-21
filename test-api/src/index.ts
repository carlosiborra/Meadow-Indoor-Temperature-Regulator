import { Hono } from 'hono'
import { cors } from 'hono/cors'

const app = new Hono()

app.use('/*', cors())

app.get('/temp', (c) => {
    let temp_min: number[] = []
    let temp_max: number[] = []
    let temperatures: number[] = []
    let timestamp: number[] = []
    const N = 15
    for (let i = 0; i < N; i++) {
        let min = i / (N/10) + 12
        let max = i / (N/10) + 15
        temp_min.push(min)
        temp_max.push(max)
        temperatures.push(min + Math.random() * (max - min))
        timestamp.push(Date.now() + i)
    }
    return c.json({ temp_min, temp_max, temperatures, timestamp })
})

export default app
