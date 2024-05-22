import type { APIRoute } from 'astro';
import { logger } from '@/lib/logger';

export const POST: APIRoute = async ({ request }) => {
    if (request.headers.get("Content-Type") === "application/json") {
        const body = await request.json();
        const { data } = body;
        console.log(data)
        logger.info(JSON.stringify(data))
    }
    return new Response()
} 