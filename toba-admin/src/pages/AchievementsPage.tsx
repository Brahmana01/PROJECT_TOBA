import React, { useEffect, useState } from 'react';
import { Trophy, Plus, Edit2, Trash2, RefreshCw, X, Check } from 'lucide-react';
import { adminGetAchievements, adminCreateAchievement, adminUpdateAchievement, adminDeleteAchievement } from '../services/api';
import type { Achievement } from '../types';
import { styles } from '../styles/shared';

const CONDITIONS = ['LEVEL','STAR','XP','LOCATION','TRIVIA','BATTLE'];
const CONDITION_LABELS: Record<string,string> = {
  LEVEL:'Capai Level', STAR:'Kumpulkan Bintang', XP:'Kumpulkan XP',
  LOCATION:'Selesaikan Lokasi', TRIVIA:'Main Trivia', BATTLE:'Menang Battle'
};

const AchievementModal = ({ ach, onClose, onSave }: {
  ach: Achievement | null; onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!ach;
  const [form, setForm] = useState({
    name: ach?.name ?? '', description: ach?.description ?? '', icon: ach?.icon ?? '',
    condition_type: ach?.condition_type ?? 'LEVEL', condition_value: ach?.condition_value ?? 1,
    reward_xp: ach?.reward_xp ?? 0, reward_stars: ach?.reward_stars ?? 0,
    is_active: ach?.is_active ?? true,
  });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSave = async () => {
    if (!form.name || !form.description) { setMsg('Nama dan deskripsi wajib.'); return; }
    setSaving(true);
    try {
      if (isEdit) await adminUpdateAchievement(ach!.id, form);
      else await adminCreateAchievement(form);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Achievement' : 'Tambah Achievement'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Nama Achievement *</label>
              <input style={styles.input} value={form.name} onChange={e => setForm(f => ({...f, name: e.target.value}))} placeholder="Penjelajah Pemula"/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Icon (emoji)</label>
              <input style={styles.input} value={form.icon} onChange={e => setForm(f => ({...f, icon: e.target.value}))} placeholder="🏆"/>
            </div>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Deskripsi *</label>
            <input style={styles.input} value={form.description} onChange={e => setForm(f => ({...f, description: e.target.value}))} placeholder="Selesaikan 1 lokasi Jelajah Budaya"/>
          </div>
          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Tipe Kondisi</label>
              <select style={{...styles.input}} value={form.condition_type} onChange={e => setForm(f => ({...f, condition_type: e.target.value}))}>
                {CONDITIONS.map(c => <option key={c} value={c}>{CONDITION_LABELS[c]}</option>)}
              </select>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Nilai Kondisi</label>
              <input style={styles.input} type="number" value={form.condition_value} onChange={e => setForm(f => ({...f, condition_value: +e.target.value}))}/>
            </div>
          </div>
          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Reward XP</label>
              <input style={styles.input} type="number" value={form.reward_xp} onChange={e => setForm(f => ({...f, reward_xp: +e.target.value}))}/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Reward Bintang</label>
              <input style={styles.input} type="number" value={form.reward_stars} onChange={e => setForm(f => ({...f, reward_stars: +e.target.value}))}/>
            </div>
          </div>
          {isEdit && (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <input type="checkbox" checked={form.is_active} onChange={e => setForm(f => ({...f, is_active: e.target.checked}))} id="achActive"/>
              <label htmlFor="achActive" style={styles.label}>Aktif</label>
            </div>
          )}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}><Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}</button>
        </div>
      </div>
    </div>
  );
};

const AchievementsPage: React.FC = () => {
  const [achievements, setAchievements] = useState<Achievement[]>([]);
  const [loading, setLoading]   = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editAch, setEditAch]   = useState<Achievement | null>(null);

  const fetch = () => {
    setLoading(true);
    adminGetAchievements().then(r => setAchievements(r.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleDelete = async (id: number) => {
    if (!confirm('Nonaktifkan achievement ini?')) return;
    await adminDeleteAchievement(id);
    fetch();
  };

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><Trophy size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Achievement</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>{achievements.length} achievement</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}><RefreshCw size={14}/> Refresh</button>
          <button onClick={() => { setEditAch(null); setShowModal(true); }} style={styles.btnPrimary}>
            <Plus size={14}/> Tambah Achievement
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 16 }}>
        {loading ? (
          <div className="wood-panel" style={{ gridColumn: '1/-1', textAlign: 'center', padding: 40, opacity: 0.5 }}>Memuat...</div>
        ) : achievements.map(ach => (
          <div key={ach.id} className="wood-panel" style={{ position: 'relative' }}>
            <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start' }}>
              <div style={{ fontSize: 40, lineHeight: 1 }}>{ach.icon || '🏆'}</div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 'bold', color: '#D4AF37', fontSize: '0.95rem' }}>{ach.name}</div>
                <div style={{ fontSize: '0.75rem', opacity: 0.7, marginTop: 4 }}>{ach.description}</div>
              </div>
              <div style={{ display: 'flex', gap: 4, flexShrink: 0 }}>
                <button onClick={() => { setEditAch(ach); setShowModal(true); }} style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}><Edit2 size={14}/></button>
                <button onClick={() => handleDelete(ach.id)} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}><Trash2 size={14}/></button>
              </div>
            </div>

            <div style={{ marginTop: 12, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6, fontSize: '0.75rem' }}>
              <div style={{ background: 'rgba(0,0,0,0.3)', padding: '6px 10px', borderRadius: 6 }}>
                🎯 {CONDITION_LABELS[ach.condition_type] ?? ach.condition_type}: <strong>{ach.condition_value}</strong>
              </div>
              <div style={{ background: 'rgba(0,0,0,0.3)', padding: '6px 10px', borderRadius: 6 }}>
                ✨ +{ach.reward_xp} XP  ⭐ +{ach.reward_stars}
              </div>
            </div>

            <div style={{ marginTop: 8 }}>
              <span className={`badge ${ach.is_active ? 'green' : 'orange'}`}>{ach.is_active ? 'Aktif' : 'Nonaktif'}</span>
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <AchievementModal ach={editAch} onClose={() => setShowModal(false)} onSave={() => { setShowModal(false); fetch(); }}/>
      )}
    </div>
  );
};

export default AchievementsPage;