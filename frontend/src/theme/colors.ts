import type { MantineColorsTuple } from '@mantine/core'

// AscendDev Brand Colors
export const brandColors: MantineColorsTuple = [
    '#e3f2fd', // lightest
    '#bbdefb',
    '#90caf9',
    '#64b5f6',
    '#42a5f5',
    '#2196f3', // primary brand color
    '#1e88e5',
    '#1976d2',
    '#1565c0',
    '#0d47a1', // darkest
]

// Success Colors (Green)
export const successColors: MantineColorsTuple = [
    '#e8f5e8',
    '#c8e6c9',
    '#a5d6a7',
    '#81c784',
    '#66bb6a',
    '#4caf50',
    '#43a047',
    '#388e3c',
    '#2e7d32',
    '#1b5e20',
]

// Warning Colors (Amber)
export const warningColors: MantineColorsTuple = [
    '#fff8e1',
    '#ffecb3',
    '#ffe082',
    '#ffd54f',
    '#ffca28',
    '#ffc107',
    '#ffb300',
    '#ffa000',
    '#ff8f00',
    '#ff6f00',
]

// Error Colors (Red)
export const errorColors: MantineColorsTuple = [
    '#ffebee',
    '#ffcdd2',
    '#ef9a9a',
    '#e57373',
    '#ef5350',
    '#f44336',
    '#e53935',
    '#d32f2f',
    '#c62828',
    '#b71c1c',
]

// Code Editor Colors
export const codeColors: MantineColorsTuple = [
    '#f8f8f2', // background light
    '#f4f4f4',
    '#e6e6e6',
    '#d4d4d4',
    '#a6a6a6',
    '#75715e', // comments
    '#66d9ef', // keywords
    '#a6e22e', // strings
    '#f92672', // operators
    '#272822', // background dark
]

// Semantic color mappings
export const semanticColors = {
    primary: brandColors[5],
    success: successColors[5],
    warning: warningColors[5],
    error: errorColors[5],
    info: brandColors[3],
} as const