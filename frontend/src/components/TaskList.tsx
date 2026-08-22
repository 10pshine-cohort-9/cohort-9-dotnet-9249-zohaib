import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { tasksApi } from '../api/api';
import type { PagedResult, Task } from '../types';

const statusOptions = ['', 'Pending', 'InProgress', 'Completed'];
const priorityOptions = ['', 'Low', 'Medium', 'High'];

export default function TaskList() {
  const [result, setResult] = useState<PagedResult<Task> | null>(null);
  const [filters, setFilters] = useState({
    search: '',
    status: '',
    priority: '',
    category: '',
    page: 1,
    pageSize: 10,
  });
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  const loadTasks = useCallback(async (query = filters) => {
    setIsLoading(true);
    setError('');
    try {
      const data = await tasksApi.getTasks({
        page: query.page,
        pageSize: query.pageSize,
        search: query.search || undefined,
        status: query.status || undefined,
        priority: query.priority || undefined,
        category: query.category || undefined,
      });
      setResult(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks.');
    } finally {
      setIsLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    loadTasks();
  }, [filters.page, filters.pageSize, loadTasks]);

  const handleFilterSubmit = (event: FormEvent) => {
    event.preventDefault();
    const nextFilters = { ...filters, page: 1 };
    setFilters(nextFilters);
    loadTasks(nextFilters);
  };

  return (
    <section className="task-list-section">
      <div className="section-header">
        <h2>All Tasks</h2>
        <p className="muted">Search, filter, and manage your tasks.</p>
      </div>

      <form className="filter-bar" onSubmit={handleFilterSubmit}>
        <input
          placeholder="Search title, description, category"
          value={filters.search}
          onChange={(e) => setFilters({ ...filters, search: e.target.value })}
        />
        <select
          value={filters.status}
          onChange={(e) => setFilters({ ...filters, status: e.target.value })}
        >
          {statusOptions.map((option) => (
            <option key={option || 'all-status'} value={option}>
              {option || 'All Statuses'}
            </option>
          ))}
        </select>
        <select
          value={filters.priority}
          onChange={(e) => setFilters({ ...filters, priority: e.target.value })}
        >
          {priorityOptions.map((option) => (
            <option key={option || 'all-priority'} value={option}>
              {option || 'All Priorities'}
            </option>
          ))}
        </select>
        <input
          placeholder="Category"
          value={filters.category}
          onChange={(e) => setFilters({ ...filters, category: e.target.value })}
        />
        <button type="submit" className="btn btn-secondary">
          Apply Filters
        </button>
      </form>

      {error && <div className="alert alert-error">{error}</div>}

      {isLoading ? (
        <div className="loading">Loading tasks...</div>
      ) : (
        <>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Category</th>
                  <th>Assigned To</th>
                  <th>Due Date</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {result?.items.length ? (
                  result.items.map((task) => (
                    <tr key={task.id}>
                      <td>{task.title}</td>
                      <td>
                        <span className={`badge badge-${task.status.toLowerCase()}`}>{task.status}</span>
                      </td>
                      <td>{task.priority}</td>
                      <td>{task.category || '-'}</td>
                      <td>{task.assignedUserName}</td>
                      <td>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}</td>
                      <td>
                        <Link to={`/tasks/${task.id}`}>View</Link>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="empty-row">
                      No tasks found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="pagination">
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              disabled={filters.page <= 1}
              onClick={() => setFilters({ ...filters, page: filters.page - 1 })}
            >
              Previous
            </button>
            <span>
              Page {result?.page ?? 1} of {result?.totalPages ?? 1} ({result?.totalCount ?? 0} tasks)
            </span>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              disabled={!result || filters.page >= result.totalPages}
              onClick={() => setFilters({ ...filters, page: filters.page + 1 })}
            >
              Next
            </button>
          </div>
        </>
      )}
    </section>
  );
}
