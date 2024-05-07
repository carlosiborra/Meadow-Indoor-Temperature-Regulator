# 🌡 Temperature Warrior frontend

## Usage
```bash
# Create a .env file
cp .env.example .env

# Poblate .env file
nano .env

# Install dependencies
pnpm i

# Start local dev server
pnpm dev
```

## 🚀 Project Structure

Inside the project, you'll see the following folders and files:

```text
/
├── public/
│   └── favicon.svg
├── src/
│   ├── components/
│   │   └── NavBar.astro
│   ├── layouts/
│   │   └── Layout.astro
│   └── pages/
│       └── index.astro
└── package.json
```

Any static assets, like images, can be placed in the `public/` directory.

## 🧞 Commands

All commands are run from the root of the project, from a terminal:

| Command                   | Action                                           |
| :------------------------ | :----------------------------------------------- |
| `pnpm i`             | Installs dependencies                            |
| `pnpm add <dependency>` | Installs <dependency> in the project |
| `pnpm dev`             | Starts local dev server at `localhost:4321`      |
| `pnpm build`           | Build your production site to `./dist/`          |
| `pnpm preview`         | Preview your build locally, before deploying     |
| `pnpm run astro ...`       | Run CLI commands like `astro add`, `astro check` |
| `pnpm run astro -- --help` | Get help using the Astro CLI                     |
| `pnpm serve --port <port>` | Builds the project and serve it into an especific port. `4321` will be used if not specified. If the port is in usage, next one will be used |

## 👀 Want to learn more?

Feel free to check [our documentation](https://docs.astro.build) or jump into our [Discord server](https://astro.build/chat).
