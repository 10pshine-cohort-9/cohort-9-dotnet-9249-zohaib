import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { tasksApi } from '../api/api';
import type { Task } from '../types';

export default function TaskDetailPage() {
  const { id } = useParams();
  const taskId = id ? Number(id) : undefined;
  const navigate = useNavigate();
  const [task, setTask] = useState<Task | null>(null);
  const [error, setError] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    if (taskId === undefined) return;
    tasksApi
      .getTask(taskId)
      .then(setTask)
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load task.'));
  }, [taskId]);

  const handleDelete = async () => {
    if (taskId === undefined) return;
    setIsDeleting(true);
    try {
      await tasksApi.deleteTask(taskId);
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete task.');
      setIsDeleting(false);
    }
  };

  if (!task && !error) return <div className="loading">Loading task...</div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Task Detail</h1>
          <p className="muted">View full task information.</p>
        </div>
        <div className="button-row">
          {task && (
            <>
              <Link to={`/tasks/${task.id}/edit`} className="btn btn-secondary">
                Edit
              </Link>
              <button type="button" className="btn btn-danger" onClick={handleDelete} disabled={isDeleting}>
                {isDeleting ? 'Deleting...' : 'Delete'}
              </button>
            </>
          )}
          <Link to="/dashboard" className="btn btn-secondary">
            Back to Dashboard
          </Link>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {task && (
        <section className="detail-card">
          <h2>{task.title}</h2>
          <div className="detail-grid">
            <div>
              <span className="label">Status</span>
              <span className={`badge badge-${task.status.toLowerCase()}`}>{task.status}</span>
            </div>
            <div>
              <span className="label">Priority</span>
              <p>{task.priority}</p>
            </div>
            <div>
              <span className="label">Category</span>
              <p>{task.category || '-'}</p>
            </div>
            <div>
              <span className="label">Due Date</span>
              <p>{task.dueDate ? new Date(task.dueDate).toLocaleString() : '-'}</p>
            </div>
            <div>
              <span className="label">Assigned To</span>
              <p>{task.assignedUserName}</p>
            </div>
            <div>
              <span className="label">Created By</span>
              <p>{task.createdByName}</p>
            </div>
            <div>
              <span className="label">Created At</span>
              <p>{new Date(task.createdAt).toLocaleString()}</p>
            </div>
            <div>
              <span className="label">Updated At</span>
              <p>{task.updatedAt ? new Date(task.updatedAt).toLocaleString() : '-'}</p>
            </div>
          </div>
          <div className="detail-description">
            <span className="label">Description</span>
            <p>{task.description || 'No description provided.'}</p>
          </div>
        </section>
      )}
    </div>
  );
}
