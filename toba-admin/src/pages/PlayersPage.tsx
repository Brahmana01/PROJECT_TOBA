import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Users, Search, RefreshCw } from 'lucide-react';
import { getAllPlayers } from '../services/api';
import type { Player } from '../types';

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debounced;
}

const PlayersPage: React.FC = () => {
  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  // Pagination
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 20;

  // Debounce search 300ms
  const debouncedSearch = useDebounce(search, 300);

  const loadPlayers = useCallback(() => {
    return getAllPlayers()
      .then(res => {
        setPlayers(res.data?.players || []);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const refreshPlayers = () => {
    setPage(1);
    setLoading(true);
    void loadPlayers();
  };

  useEffect(() => {
    void loadPlayers();
  }, [loadPlayers]);

  // Filter is derived from the current player data and debounced search term.
  const filtered = useMemo(() => {
    const q = debouncedSearch.toLowerCase();
    return players.filter(player =>
      player.name.toLowerCase().includes(q) ||
      player.username.toLowerCase().includes(q)
    );
  }, [debouncedSearch, players]);

  // Keep the displayed page valid without synchronizing derived state in an effect.
  const totalPages = Math.ceil(filtered.length / PAGE_SIZE);
  const currentPage = Math.max(1, Math.min(page, totalPages || 1));

  const paginated = filtered.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  );

  return (
    <div className="dashboard-container">
      <div className="welcome-header">
        <div className="welcome-profile">
          <div className="stat-icon-wrap">
            <Users size={24} />
          </div>

          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>
              Manajemen Pemain
            </h2>

            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>
              Total {players.length} pemain terdaftar
            </p>
          </div>
        </div>

        <button
          onClick={refreshPlayers}
          style={{
            background: 'rgba(212,175,55,0.1)',
            border: '1px solid #D4AF37',
            color: '#D4AF37',
            padding: '8px 16px',
            borderRadius: '8px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          <RefreshCw size={16} />
          Refresh
        </button>
      </div>

      <div className="wood-panel">
        {/* Search */}
        <div
          style={{
            display: 'flex',
            gap: '12px',
            marginBottom: '20px',
            alignItems: 'center'
          }}
        >
          <div
            style={{
              position: 'relative',
              flex: 1
            }}
          >
            <Search
              size={16}
              style={{
                position: 'absolute',
                left: '12px',
                top: '50%',
                transform: 'translateY(-50%)',
                color: '#D4AF37'
              }}
            />

            <input
              type="text"
              placeholder="Cari nama atau username..."
              value={search}
              onChange={e => { setSearch(e.target.value); setPage(1); }}
              style={{
                width: '100%',
                padding: '10px 10px 10px 38px',
                background: 'rgba(0,0,0,0.3)',
                border: '1px solid #5C3B26',
                borderRadius: '8px',
                color: '#F5F5DC',
                fontSize: '0.85rem',
                outline: 'none',
                boxSizing: 'border-box'
              }}
            />
          </div>

          <span
            style={{
              fontSize: '0.8rem',
              opacity: 0.6
            }}
          >
            {filtered.length} hasil
          </span>
        </div>

        {/* Tabel */}
        <div style={{ overflowX: 'auto' }}>
          <table
            className="toba-table-luxury"
            style={{ width: '100%' }}
          >
            <thead>
              <tr>
                <th>#</th>
                <th>Nama</th>
                <th>Username</th>
                <th>Kode</th>
                <th>Level</th>
                <th>XP</th>
                <th>Bintang</th>
                <th>Status</th>
                <th>Bergabung</th>
              </tr>
            </thead>

            <tbody>
              {loading ? (
                <tr>
                  <td
                    colSpan={9}
                    style={{
                      textAlign: 'center',
                      padding: '40px',
                      opacity: 0.5
                    }}
                  >
                    Memuat data...
                  </td>
                </tr>
              ) : filtered.length === 0 ? (
                <tr>
                  <td
                    colSpan={9}
                    style={{
                      textAlign: 'center',
                      padding: '40px',
                      opacity: 0.5
                    }}
                  >
                    Tidak ada pemain ditemukan
                  </td>
                </tr>
              ) : (
                paginated.map((p, i) => (
                  <tr key={p.id}>
                    <td style={{ opacity: 0.5 }}>
                      {(page - 1) * PAGE_SIZE + i + 1}
                    </td>

                    <td>
                      <div
                        style={{
                          display: 'flex',
                          alignItems: 'center',
                          gap: '10px'
                        }}
                      >
                        <div
                          style={{
                            width: 32,
                            height: 32,
                            borderRadius: '50%',
                            background: '#D4AF37',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            fontWeight: 'bold',
                            color: '#2C1810',
                            fontSize: '0.8rem'
                          }}
                        >
                          {p.name.charAt(0).toUpperCase()}
                        </div>

                        {p.name}
                      </div>
                    </td>

                    <td style={{ opacity: 0.7 }}>
                      @{p.username}
                    </td>

                    <td>
                      <span
                        style={{
                          fontFamily: 'monospace',
                          fontSize: '0.75rem',
                          opacity: 0.6
                        }}
                      >
                        {p.user_code}
                      </span>
                    </td>

                    <td>
                      <span className="badge blue">
                        Lv.{p.level}
                      </span>
                    </td>

                    <td>
                      {p.xp?.toLocaleString() ?? 0} XP
                    </td>

                    <td>
                      ⭐ {p.stars}
                    </td>

                    <td>
                      <span
                        className={`badge ${
                          p.is_active ? 'green' : 'orange'
                        }`}
                      >
                        {p.is_active ? 'Aktif' : 'Nonaktif'}
                      </span>
                    </td>

                    <td
                      style={{
                        opacity: 0.6,
                        fontSize: '0.75rem'
                      }}
                    >
                      {new Date(p.created_at).toLocaleDateString(
                        'id-ID'
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div
            style={{
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              gap: '8px',
              marginTop: '16px'
            }}
          >
            <button
              onClick={() =>
                setPage(Math.max(1, currentPage - 1))
              }
              disabled={currentPage === 1}
              style={{
                background: 'rgba(212,175,55,0.1)',
                border: '1px solid #D4AF37',
                color: '#D4AF37',
                padding: '8px 14px',
                borderRadius: '8px',
                cursor: currentPage === 1 ? 'not-allowed' : 'pointer',
                opacity: currentPage === 1 ? 0.4 : 1
              }}
            >
              ← Prev
            </button>

            <span
              style={{
                alignSelf: 'center',
                fontSize: '0.85rem',
                minWidth: '60px',
                textAlign: 'center'
              }}
            >
              {currentPage} / {totalPages}
            </span>

            <button
              onClick={() =>
                setPage(Math.min(totalPages, currentPage + 1))
              }
              disabled={currentPage === totalPages}
              style={{
                background: 'rgba(212,175,55,0.1)',
                border: '1px solid #D4AF37',
                color: '#D4AF37',
                padding: '8px 14px',
                borderRadius: '8px',
                cursor:
                  currentPage === totalPages
                    ? 'not-allowed'
                    : 'pointer',
                opacity: currentPage === totalPages ? 0.4 : 1
              }}
            >
              Next →
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayersPage;