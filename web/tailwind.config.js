/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],
  content: ["./src/**/*.{astro,html,js,jsx,md,mdx,svelte,ts,tsx,vue}"],
  prefix: "",
  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
        'fountain-blue': {
          '50': '#ebfffc',
          '100': '#cdfffb',
          '200': '#a1fffa',
          '300': '#60fff9',
          '400': '#18f8f4',
          '500': '#00dede',
          '600': '#00adb5',
          '700': '#088c96',
          '800': '#10707a',
          '900': '#125d67',
          '950': '#053e47',
        },
        'guardsman-red': {
          '50': '#fff0ef',
          '100': '#ffdddc',
          '200': '#ffc1bf',
          '300': '#ff9692',
          '400': '#ff5b54',
          '500': '#ff281f',
          '600': '#ff0a00',
          '700': '#db0800',
          '800': '#b50700',
          '900': '#940d08',
          '950': '#520300',
        },
        'dark-primary': '#222831',
        'dark-secondary': '#393e46',
        'light-primary': '#eeeeee',
      },
      placeholderColor: theme => ({
        ...theme('colors'),
        'fountain-blue-opacity-50': `rgba(0, 222, 222, 0.5)`,
        'guardsman-red-opacity-50': `rgba(255, 40, 31, 0.5)`,
      }),
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
      keyframes: {
        "accordion-down": {
          from: { height: "0" },
          to: { height: "var(--radix-accordion-content-height)" },
        },
        "accordion-up": {
          from: { height: "var(--radix-accordion-content-height)" },
          to: { height: "0" },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
      },
    },
  },
  plugins: [require("tailwindcss-animate"), require('daisyui')],
}