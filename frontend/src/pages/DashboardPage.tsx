import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import TaskList from '../components/TaskList';
import { tasksApi } from '../api/api';
import { useAuth } from '../context/AuthContext';
import type { DashboardStats } from '../types';

export default function DashboardPage() {
  const { isAdmin } = useAuth();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    tasksApi
      .getDashboardStats()
      .then(setStats)
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load dashboard.'));
  }, []);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Dashboard</h1>
          <p className="muted">
            {isAdmin ? 'Overview of all tasks in the system.' : 'Your task summary and task list.'}
          </p>
        </div>
        <Link to="/tasks/new" className="btn btn-primary">
          Create Task
        </Link>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="stats-grid">
        <article className="stat-card pending">
          <span>Pending</span>
          <strong>{stats?.pendingCount ?? 0}</strong>
        </article>
        <article className="stat-card in-progress">
          <span>In Progress</span>
          <strong>{stats?.inProgressCount ?? 0}</strong>
        </article>
        <article className="stat-card completed">
          <span>Completed</span>
          <strong>{stats?.completedCount ?? 0}</strong>
        </article>
      </div>

      <TaskList />
    </div>
  );
}
