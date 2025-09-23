import { Provider } from 'react-redux'
import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { MantineProvider, ColorSchemeScript } from '@mantine/core'
import { ModalsProvider } from '@mantine/modals'
import { Notifications } from '@mantine/notifications'
import { store } from './store'
import { queryClient } from './lib/queryClient'
import { router } from './router'
import { theme } from './theme'
import { useAuthRestore } from './hooks/useAuthRestore'
import { useSignalR } from './hooks/useSignalR'

// Import Mantine styles
import '@mantine/core/styles.css'
import '@mantine/notifications/styles.css'
import '@mantine/code-highlight/styles.css'
import 'mantine-datatable/styles.css'

function AppContent() {
  useAuthRestore()
  useSignalR()

  return (
    <MantineProvider theme={theme} defaultColorScheme="dark">
      <ModalsProvider>
        <Notifications position="top-right" />
        <RouterProvider router={router} />
      </ModalsProvider>
    </MantineProvider>
  )
}

function App() {
  return (
    <>
      <ColorSchemeScript />
      <Provider store={store}>
        <QueryClientProvider client={queryClient}>
          <AppContent />
        </QueryClientProvider>
      </Provider>
    </>
  )
}

export default App
