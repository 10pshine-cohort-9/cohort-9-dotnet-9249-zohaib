import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { user, logout } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <Link to="/dashboard">Task Management</Link>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/dashboard" end>
            Dashboard
          </NavLink>
          <NavLink to="/tasks/new">New Task</NavLink>
          <NavLink to="/profile">Profile</NavLink>
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-user">
            <span className="sidebar-user-name">{user?.name}</span>
          </div>
          <button type="button" className="btn btn-secondary btn-sm btn-block" onClick={() => void logout()}>
            Logout
          </button>
        </div>
      </aside>

      <div className="app-body">
        <main className="main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
