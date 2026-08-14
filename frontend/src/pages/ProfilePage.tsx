import { useAuth } from '../context/AuthContext';

export default function ProfilePage() {
  const { user, logout } = useAuth();

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>User Profile</h1>
          <p className="muted">Your account details</p>
        </div>
        <button type="button" className="btn btn-danger" onClick={() => void logout()}>
          Logout
        </button>
      </div>

      <section className="detail-card">
        <div className="detail-grid">
          <div>
            <span className="label">Name</span>
            <p>{user?.name}</p>
          </div>
          <div>
            <span className="label">Email</span>
            <p>{user?.email}</p>
          </div>
          <div>
            <span className="label">Role</span>
            <p>{user?.role}</p>
          </div>
        </div>
      </section>
    </div>
  );
}
