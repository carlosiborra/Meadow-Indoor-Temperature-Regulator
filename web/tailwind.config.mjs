/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{astro,html,js,jsx,md,mdx,svelte,ts,tsx,vue}'],
	theme: {
		extend: {
            colors: {
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
              })
        },
	},
	plugins: [require('daisyui')],
}
