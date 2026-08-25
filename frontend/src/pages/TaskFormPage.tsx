import { type FormEvent, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { tasksApi } from '../api/api';
import { useAuth } from '../context/AuthContext';
import type { TaskFormData, User } from '../types';

const defaultForm: TaskFormData = {
  title: '',
  description: '',
  status: 'Pending',
  priority: 'Medium',
  category: '',
  dueDate: '',
  assignedUserId: undefined,
};

export default function TaskFormPage() {
  const { id } = useParams();
  const isEdit = Boolean(id);
  const taskId = id ? Number(id) : undefined;
  const navigate = useNavigate();
  const { user, isAdmin } = useAuth();
  const [form, setForm] = useState<TaskFormData>(defaultForm);
  const [users, setUsers] = useState<User[]>([]);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(isEdit);

  useEffect(() => {
    tasksApi.getAssignableUsers().then(setUsers).catch(() => setUsers([]));
  }, []);

  useEffect(() => {
    if (!isEdit || taskId === undefined) {
      if (user) {
        setForm((prev) => ({ ...prev, assignedUserId: user.id }));
      }
      return;
    }

    setIsLoading(true);
    tasksApi
      .getTask(taskId)
      .then((task) => {
        setForm({
          title: task.title,
          description: task.description || '',
          status: task.status,
          priority: task.priority,
          category: task.category || '',
          dueDate: task.dueDate ? task.dueDate.slice(0, 16) : '',
          assignedUserId: task.assignedUserId,
        });
      })
      .catch((err) => setSubmitError(err instanceof Error ? err.message : 'Failed to load task.'))
      .finally(() => setIsLoading(false));
  }, [id, isEdit, user]);

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!form.title.trim() || form.title.trim().length < 3) {
      nextErrors.title = 'Title must be at least 3 characters.';
    }
    if (isAdmin && !form.assignedUserId) {
      nextErrors.assignedUserId = 'Assigned user is required.';
    }
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSubmitError('');
    if (!validate()) return;

    setIsSubmitting(true);
    const payload: TaskFormData = {
      ...form,
      title: form.title.trim(),
      description: form.description?.trim(),
      category: form.category?.trim(),
      dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : undefined,
      assignedUserId: isAdmin ? form.assignedUserId : user?.id,
    };

    try {
      if (isEdit && taskId !== undefined) {
        await tasksApi.updateTask(taskId, payload);
        navigate(`/tasks/${id}`);
      } else {
        const created = await tasksApi.createTask(payload);
        navigate(`/tasks/${created.id}`);
      }
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Failed to save task.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <div className="loading">Loading task...</div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>{isEdit ? 'Edit Task' : 'New Task'}</h1>
          <p className="muted">
            {isAdmin
              ? 'Create or update tasks and assign them to users.'
              : 'Create or update tasks assigned to yourself.'}
          </p>
        </div>
        <Link to="/dashboard" className="btn btn-secondary">
          Back to Dashboard
        </Link>
      </div>

      <form className="form-card" onSubmit={handleSubmit} noValidate>
        <label>
          Title
          <input
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            placeholder="Task title"
          />
          {errors.title && <span className="field-error">{errors.title}</span>}
        </label>

        <label>
          Description
          <textarea
            rows={4}
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            placeholder="Task details"
          />
        </label>

        <div className="form-grid">
          <label>
            Status
            <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
              <option value="Pending">Pending</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
            </select>
          </label>

          <label>
            Priority
            <select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </label>

          <label>
            Category
            <input
              value={form.category}
              onChange={(e) => setForm({ ...form, category: e.target.value })}
              placeholder="e.g. Development"
            />
          </label>

          <label>
            Due Date
            <input
              type="datetime-local"
              value={form.dueDate}
              onChange={(e) => setForm({ ...form, dueDate: e.target.value })}
            />
          </label>

          {isAdmin && (
            <label>
              Assign To
              <select
                value={form.assignedUserId ?? ''}
                onChange={(e) =>
                  setForm({
                    ...form,
                    assignedUserId: e.target.value ? Number(e.target.value) : undefined,
                  })
                }
              >
                <option value="">Select user</option>
                {users.map((assignee) => (
                  <option key={assignee.id} value={assignee.id}>
                    {assignee.name} ({assignee.email})
                  </option>
                ))}
              </select>
              {errors.assignedUserId && <span className="field-error">{errors.assignedUserId}</span>}
            </label>
          )}
        </div>

        {submitError && <div className="alert alert-error">{submitError}</div>}

        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : isEdit ? 'Update Task' : 'Create Task'}
        </button>
      </form>
    </div>
  );
}
