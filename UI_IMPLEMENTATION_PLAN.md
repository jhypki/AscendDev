# AscendDev Frontend Implementation Plan

## Overview
This document provides a comprehensive, step-by-step plan for implementing a modern React frontend for the AscendDev e-learning platform using Vite, TypeScript, TanStack Query, Mantine UI, and Redux Toolkit.

## Technology Stack Analysis

### Core Technologies
- **Vite + React + TypeScript**: Modern build tool with fast HMR and excellent TypeScript support
- **TanStack Query v5**: Powerful data fetching and server state management
- **Mantine UI**: Fully featured React components library with 120+ customizable components
- **Redux Toolkit**: Modern Redux with simplified API and built-in best practices
- **Mantine Styles**: Built-in styling system with theme support and CSS-in-JS

### API Analysis
Based on the backend analysis, the platform includes:

**Core Controllers:**
- `AuthController`: Authentication (login, register, refresh tokens)
- `CoursesController`: Course management with full CRUD operations
- `CodeExecutionController`: Code execution and testing
- `TestsController`: Running tests for lessons
- `SubmissionsController`: User code submissions
- `DiscussionsController`: Social features and discussions
- `UserManagementController`: Admin user management
- `UserSettingsController`: User preferences
- `RoleManagementController`: Role-based access control

**Key Features to Implement:**
1. Authentication & Authorization
2. Course browsing and management
3. Interactive code editor with execution
4. Progress tracking and submissions
5. Discussion forums
6. User dashboard and settings
7. Admin panel (role-based)

## Project Structure

```
frontend/
├── public/
│   ├── index.html
│   └── favicon.ico
├── src/
│   ├── components/           # Reusable UI components
│   │   ├── layout/          # Layout components
│   │   ├── forms/           # Form components
│   │   ├── code-editor/     # Code editor components
│   │   └── common/          # Common components
│   ├── pages/               # Page components
│   │   ├── auth/            # Authentication pages
│   │   ├── courses/         # Course-related pages
│   │   ├── lessons/         # Lesson pages
│   │   ├── dashboard/       # User dashboard
│   │   ├── admin/           # Admin pages
│   │   └── profile/         # User profile pages
│   ├── hooks/               # Custom React hooks
│   │   ├── api/             # TanStack Query hooks
│   │   ├── auth/            # Authentication hooks
│   │   └── common/          # Common utility hooks
│   ├── store/               # Redux store configuration
│   │   ├── slices/          # Redux slices
│   │   ├── api/             # RTK Query API slices
│   │   └── index.ts         # Store configuration
│   ├── lib/                 # Utility libraries
│   │   ├── api.ts           # API client configuration
│   │   ├── auth.ts          # Authentication utilities
│   │   ├── utils.ts         # General utilities
│   │   └── constants.ts     # Application constants
│   ├── types/               # TypeScript type definitions
│   │   ├── api.ts           # API response types
│   │   ├── auth.ts          # Authentication types
│   │   └── common.ts        # Common types
│   ├── theme/               # Mantine theme configuration
│   │   ├── theme.ts         # Main theme configuration
│   │   └── colors.ts        # Custom color palette
│   ├── App.tsx              # Main App component
│   ├── main.tsx             # Application entry point
│   └── vite-env.d.ts        # Vite type definitions
├── package.json
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.node.json
├── vite.config.ts
├── postcss.config.js        # PostCSS configuration for Mantine
└── README.md
```

## Component Architecture

### Layout Components
```typescript
// Layout hierarchy
App
├── AuthProvider (Redux Provider + Auth Context)
├── QueryClientProvider (TanStack Query)
├── MantineProvider (Theme + Components)
└── Router
    ├── PublicLayout
    │   ├── Header
    │   ├── Navigation
    │   └── Footer
    ├── AuthenticatedLayout
    │   ├── Sidebar
    │   ├── Header
    │   └── MainContent
    └── AdminLayout
        ├── AdminSidebar
        ├── AdminHeader
        └── AdminContent
```

### Page Components
- **Authentication**: Login, Register, ForgotPassword
- **Courses**: CourseList, CourseDetail, CourseCreate, CourseEdit
- **Lessons**: LessonView, LessonEdit, CodeEditor
- **Dashboard**: UserDashboard, Progress, Submissions
- **Admin**: UserManagement, CourseManagement, Analytics
- **Profile**: UserProfile, Settings, Preferences

### UI Components (Mantine based)
- **Forms**: TextInput, Textarea, Select, Checkbox, Radio, Switch, NumberInput
- **Navigation**: Breadcrumbs, Tabs, Pagination, Stepper, NavLink
- **Feedback**: Alert, Notification, Progress, Skeleton, Loader
- **Overlay**: Modal, Drawer, Popover, Tooltip, Menu
- **Data Display**: Table, Card, Badge, Avatar, Timeline, Spotlight
- **Layout**: Grid, SimpleGrid, Stack, Group, Container, AppShell

## State Management Strategy

### Redux Toolkit Setup
```typescript
// store/index.ts
import { configureStore } from '@reduxjs/toolkit'
import { authSlice } from './slices/authSlice'
import { uiSlice } from './slices/uiSlice'
import { api } from './api/apiSlice'

export const store = configureStore({
  reducer: {
    auth: authSlice.reducer,
    ui: uiSlice.reducer,
    api: api.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(api.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
```

### State Structure
1. **Auth State**: User info, tokens, permissions
2. **UI State**: Theme, sidebar state, modals, loading states
3. **API State**: Managed by RTK Query for server state

### TanStack Query Integration
- Use for all server state management
- Implement optimistic updates for better UX
- Cache invalidation strategies
- Background refetching for real-time data

## API Integration Patterns

### API Client Setup
```typescript
// lib/api.ts
import axios from 'axios'
import { store } from '../store'

const api = axios.create({
  baseURL: process.env.VITE_API_URL || 'http://localhost:5000/api',
})

// Request interceptor for auth tokens
api.interceptors.request.use((config) => {
  const token = store.getState().auth.token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      store.dispatch(authSlice.actions.logout())
    }
    return Promise.reject(error)
  }
)
```

### TanStack Query Hooks Pattern
```typescript
// hooks/api/useCourses.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../../lib/api'

export const useCourses = () => {
  return useQuery({
    queryKey: ['courses'],
    queryFn: () => api.get('/courses').then(res => res.data),
  })
}

export const useCreateCourse = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (courseData) => api.post('/courses', courseData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courses'] })
    },
  })
}
```

## Step-by-Step Implementation Guide

### Phase 1: Project Setup (Days 1-2)
1. **Initialize Vite Project**
   ```bash
   npm create vite@latest ascenddev-frontend -- --template react-ts
   cd ascenddev-frontend
   npm install
   ```

2. **Install Dependencies**
   ```bash
   # Core dependencies
   npm install @tanstack/react-query @reduxjs/toolkit react-redux
   npm install axios react-router-dom
   
   # Mantine UI dependencies
   npm install @mantine/core @mantine/hooks @mantine/notifications
   npm install @mantine/modals @mantine/spotlight @mantine/dates
   npm install @mantine/form @mantine/code-highlight
   npm install -D @types/node
   
   # Code editor
   npm install @monaco-editor/react
   
   # Form handling (Mantine has built-in forms, but can use react-hook-form too)
   npm install react-hook-form @hookform/resolvers zod
   ```

3. **Setup PostCSS for Mantine**
   ```bash
   npm install -D postcss postcss-preset-mantine postcss-simple-vars
   ```

4. **Configure TypeScript paths in tsconfig.json**

### Phase 2: Core Infrastructure (Days 3-5)
1. **Setup Redux Store**
   - Configure store with RTK
   - Create auth slice
   - Create UI slice
   - Setup typed hooks

2. **Setup TanStack Query**
   - Configure QueryClient
   - Setup providers
   - Create base API hooks

3. **Setup Routing**
   - Configure React Router
   - Create route guards
   - Setup lazy loading

4. **Setup Authentication**
   - JWT token management
   - Auth context/hooks
   - Protected routes
   - Token refresh logic

### Phase 3: UI Foundation (Days 6-8)
1. **Setup Mantine Theme and Providers**
   ```typescript
   // Setup MantineProvider with custom theme
   // Configure dark/light mode
   // Setup notifications and modals providers
   ```

2. **Create Layout Components**
   - AppShell with header and navigation
   - Sidebar for authenticated users
   - Footer component
   - Theme toggle for dark/light mode

3. **Setup Mantine Styles**
   - Custom theme configuration
   - Color scheme setup
   - Component customization

### Phase 4: Authentication (Days 9-11)
1. **Auth Pages**
   - Login form with validation
   - Registration form
   - Password reset flow

2. **Auth Integration**
   - API integration
   - Token management
   - User state management
   - Route protection

### Phase 5: Course Management (Days 12-16)
1. **Course Listing**
   - Course cards with filtering
   - Search functionality
   - Pagination

2. **Course Detail**
   - Course information display
   - Lesson navigation
   - Progress tracking

3. **Course Creation/Editing** (Admin)
   - Form with validation
   - Rich text editor for descriptions
   - Image upload handling

### Phase 6: Lesson System (Days 17-21)
1. **Lesson Viewer**
   - Content display
   - Code editor integration
   - Test execution

2. **Code Editor**
   - Monaco Editor integration
   - Syntax highlighting
   - Code execution
   - Test results display

3. **Progress Tracking**
   - Submission handling
   - Progress visualization
   - Achievement system

### Phase 7: Social Features (Days 22-25)
1. **Discussion System**
   - Discussion threads
   - Reply functionality
   - Real-time updates

2. **User Profiles**
   - Profile pages
   - Activity feeds
   - Settings management

### Phase 8: Admin Panel (Days 26-28)
1. **User Management**
   - User listing and search
   - Role management
   - Bulk operations

2. **Course Management**
   - Course administration
   - Analytics dashboard
   - Content moderation

### Phase 9: Polish & Optimization (Days 29-30)
1. **Performance Optimization**
   - Code splitting
   - Image optimization
   - Bundle analysis

2. **Testing**
   - Unit tests for components
   - Integration tests
   - E2E tests

3. **Documentation**
   - Component documentation
   - API documentation
   - Deployment guide

## Key Implementation Details

### Mantine Provider Setup
```typescript
// App.tsx with Mantine providers
import { MantineProvider, createTheme } from '@mantine/core';
import { ModalsProvider } from '@mantine/modals';
import { Notifications } from '@mantine/notifications';
import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import '@mantine/code-highlight/styles.css';

const theme = createTheme({
  primaryColor: 'blue',
  defaultRadius: 'md',
  fontFamily: 'Inter, sans-serif',
});

function App() {
  return (
    <MantineProvider theme={theme}>
      <ModalsProvider>
        <Notifications />
        {/* Your app content */}
      </ModalsProvider>
    </MantineProvider>
  );
}
```

### Authentication Flow
```typescript
// Auth slice with RTK
const authSlice = createSlice({
  name: 'auth',
  initialState: {
    user: null,
    token: null,
    isAuthenticated: false,
    loading: false,
  },
  reducers: {
    loginStart: (state) => {
      state.loading = true
    },
    loginSuccess: (state, action) => {
      state.user = action.payload.user
      state.token = action.payload.token
      state.isAuthenticated = true
      state.loading = false
    },
    logout: (state) => {
      state.user = null
      state.token = null
      state.isAuthenticated = false
    },
  },
})
```

### Code Editor Integration
```typescript
// Monaco Editor component
import Editor from '@monaco-editor/react'

const CodeEditor = ({ language, value, onChange }) => {
  return (
    <Editor
      height="400px"
      language={language}
      value={value}
      onChange={onChange}
      theme="vs-dark"
      options={{
        minimap: { enabled: false },
        fontSize: 14,
        wordWrap: 'on',
      }}
    />
  )
}
```

### Form Handling with Mantine
```typescript
// Using Mantine's built-in form system
import { useForm } from '@mantine/form';
import { TextInput, PasswordInput, Button, Stack } from '@mantine/core';
import { zodResolver } from 'mantine-form-zod-resolver';
import { z } from 'zod';

const loginSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

const LoginForm = () => {
  const form = useForm({
    validate: zodResolver(loginSchema),
    initialValues: {
      email: '',
      password: '',
    },
  });

  return (
    <form onSubmit={form.onSubmit((values) => console.log(values))}>
      <Stack>
        <TextInput
          label="Email"
          placeholder="Enter your email"
          {...form.getInputProps('email')}
        />
        <PasswordInput
          label="Password"
          placeholder="Enter your password"
          {...form.getInputProps('password')}
        />
        <Button type="submit">Login</Button>
      </Stack>
    </form>
  );
};
```

### AppShell Layout Example
```typescript
// Modern layout with Mantine AppShell
import { AppShell, Burger, Group, Text, NavLink } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';

function Layout({ children }) {
  const [opened, { toggle }] = useDisclosure();

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md">
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Text size="xl" fw={700}>AscendDev</Text>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <NavLink label="Dashboard" />
        <NavLink label="Courses" />
        <NavLink label="Profile" />
      </AppShell.Navbar>

      <AppShell.Main>{children}</AppShell.Main>
    </AppShell>
  );
}
```

## Best Practices

### Code Organization
- Use barrel exports for cleaner imports
- Implement consistent naming conventions
- Create reusable custom hooks
- Use TypeScript strictly

### Performance
- Implement code splitting with React.lazy
- Use React.memo for expensive components
- Optimize TanStack Query with proper cache times
- Implement virtual scrolling for large lists

### Accessibility
- Use semantic HTML elements
- Implement proper ARIA attributes
- Ensure keyboard navigation
- Test with screen readers

### Error Handling
- Global error boundary
- API error handling
- User-friendly error messages
- Retry mechanisms

## Testing Strategy

### Unit Tests
- Component testing with React Testing Library
- Hook testing
- Utility function testing

### Integration Tests
- API integration tests
- Form submission flows
- Authentication flows

### E2E Tests
- Critical user journeys
- Cross-browser testing
- Mobile responsiveness

## Deployment Considerations

### Build Optimization
- Environment variables configuration
- Bundle size optimization
- Asset optimization

### CI/CD Pipeline
- Automated testing
- Build verification
- Deployment automation

## Why Mantine Over shadcn/ui?

### Advantages of Mantine:
1. **Complete Component Library**: 120+ pre-built components vs copy-paste approach
2. **Built-in Features**: Forms, notifications, modals, date pickers included
3. **Excellent TypeScript Support**: Full type safety out of the box
4. **Comprehensive Documentation**: 3000+ code examples and detailed guides
5. **Theme System**: Powerful theming with CSS-in-JS support
6. **Accessibility**: WCAG compliant components by default
7. **Performance**: Optimized bundle size and tree-shaking
8. **Developer Experience**: Hooks, utilities, and dev tools included
9. **Maintenance**: Single dependency vs multiple shadcn/ui components
10. **Rapid Development**: Faster implementation with pre-built components

### Key Mantine Features for AscendDev:
- **AppShell**: Perfect for dashboard layouts
- **Code Highlight**: Built-in syntax highlighting
- **Spotlight**: Command palette for navigation
- **Notifications**: Toast system included
- **Modals**: Modal management system
- **Forms**: Powerful form handling with validation
- **Data Tables**: Advanced table components
- **Charts**: Data visualization components
- **Date Pickers**: Calendar and date selection
- **Rich Text Editor**: WYSIWYG editor support

This comprehensive plan provides a structured approach to building a modern, scalable React frontend for the AscendDev platform using Mantine UI. The choice of Mantine significantly reduces development time while providing a professional, accessible, and maintainable codebase.