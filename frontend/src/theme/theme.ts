import { createTheme } from '@mantine/core'
import { brandColors, successColors, warningColors, errorColors, codeColors } from './colors'

export const theme = createTheme({
    primaryColor: 'brand',
    defaultRadius: 'md',
    fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
    headings: {
        fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        fontWeight: '600',
    },
    colors: {
        brand: brandColors,
        success: successColors,
        warning: warningColors,
        error: errorColors,
        code: codeColors,
    },
    breakpoints: {
        xs: '30em',
        sm: '48em',
        md: '64em',
        lg: '74em',
        xl: '90em',
    },
    spacing: {
        xs: '0.5rem',
        sm: '0.75rem',
        md: '1rem',
        lg: '1.5rem',
        xl: '2rem',
    },
    components: {
        Button: {
            defaultProps: {
                size: 'md',
            },
        },
        TextInput: {
            defaultProps: {
                size: 'md',
            },
        },
        Select: {
            defaultProps: {
                size: 'md',
            },
        },
        Textarea: {
            defaultProps: {
                size: 'md',
            },
        },
    },
})