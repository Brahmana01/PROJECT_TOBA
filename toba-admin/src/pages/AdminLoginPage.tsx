import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminLogin } from '../services/api';
import topOrnament from '../assets/admin/batak-top-ornament.png';
import sideOrnament from '../assets/admin/batak-side-ornament.png';
import '../styles/AdminLogin.css';

const AdminLoginPage: React.FC = () => {
  const navigate = useNavigate();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await adminLogin({ username, password });

      localStorage.setItem('adminToken', response.data.token);
      localStorage.setItem('adminData', JSON.stringify(response.data.admin));

      navigate('/dashboard');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Username atau password salah');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="admin-login-page">
      <div className="login-particles" />

      <div className="admin-login-card">
        <img src={topOrnament} className="login-top-ornament" alt="" />
        <img src={sideOrnament} className="login-side-ornament login-side-left" alt="" />
        <img src={sideOrnament} className="login-side-ornament login-side-right" alt="" />

        <div className="login-brand">
          <h1>TOBA ADMIN</h1>
          <p>Kelola Game Edukasi Budaya Batak Toba</p>
        </div>

        <div className="login-divider">
          <span>◆</span>
        </div>

        <form onSubmit={handleLogin} className="admin-login-form">
          <div className="login-input-group">
            <span className="login-input-icon">♙</span>
            <input
              type="text"
              placeholder="Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="login-input-group">
            <span className="login-input-icon">🔒</span>
            <input
              type={showPassword ? 'text' : 'password'}
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? '◉' : '◌'}
            </button>
          </div>

          {error && <div className="admin-login-error">{error}</div>}

          <button type="submit" className="admin-login-button" disabled={loading}>
            <span>{loading ? 'MEMPROSES...' : 'LOGIN'}</span>
          </button>
        </form>

        <div className="login-footer">
          <span>Admin Panel</span>
          <b>•</b>
          <span>TOBA</span>
        </div>
      </div>
    </div>
  );
};

export default AdminLoginPage;