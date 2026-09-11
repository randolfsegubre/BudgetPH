/**
 * Catch-all 404 page - wired to a `path="*"` Route in App.jsx, which
 * react-router matches when no other route matches the current URL.
 * Replaces the previous silent `<Navigate to="/" />` redirect - a bad URL
 * should tell the user it was wrong, not just quietly bounce them home.
 */
export default function NotFoundPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center text-center px-6">
      <p className="text-xs uppercase tracking-widest text-dark-400">404</p>
      <h1 className="mt-3 text-2xl font-bold text-dark-50">We couldn&apos;t find that page</h1>
      <p className="mt-2 text-dark-300 max-w-sm">
        The page you&apos;re looking for doesn&apos;t exist or may have moved.
      </p>
      <a href="/" className="btn-primary mt-8">
        Back to Home
      </a>
    </div>
  )
}
