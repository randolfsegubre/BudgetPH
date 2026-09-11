import React from 'react'

/**
 * React error boundary - the standard React pattern for catching a
 * render-time error anywhere in the component tree below it. Must be a
 * class component: componentDidCatch/getDerivedStateFromError have no
 * hook equivalent, unlike every other component in this app.
 */
export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  componentDidCatch(error, info) {
    // Real errors always go to the console for debugging - never swallowed silently.
    console.error('ErrorBoundary caught an error:', error, info)
  }

  handleBackToHome = () => {
    // A full page reload (not react-router's navigate()) is deliberate:
    // navigate() would just re-render the same broken component tree,
    // since this.state.hasError only resets on unmount. Reloading at "/"
    // restarts the app fresh with no stale in-memory state - the token in
    // authStore persists (it's backed by localStorage, not memory), so a
    // still-logged-in user lands back on the Dashboard, not logged out.
    window.location.href = '/'
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex flex-col items-center justify-center text-center px-6">
          <p className="text-xs uppercase tracking-widest text-dark-400">Something went wrong</p>
          <h1 className="mt-3 text-2xl font-bold text-dark-50">We hit a snag loading this page</h1>
          <p className="mt-2 text-dark-300 max-w-sm">
            This is on us, not you. Going back to the homepage usually clears it up.
          </p>
          <button type="button" className="btn-primary mt-8" onClick={this.handleBackToHome}>
            Back to Home
          </button>
        </div>
      )
    }

    return this.props.children
  }
}
