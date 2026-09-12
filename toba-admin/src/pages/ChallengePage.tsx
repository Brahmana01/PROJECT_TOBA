import React, { useEffect, useState } from 'react';
import { Trophy, Plus, Edit2, Trash2, RefreshCw, X, Check } from 'lucide-react';
import api from '../services/api';
import { styles } from '../styles/shared';

interface Challenge {
  id: number; title: string; description: string;
  game_type: string; reward_xp: number; reward_stars: number;
  is_active: boolean; start_date: string; end_date: string; data_json: string;
}

const GAME_TYPES = [
  { value: 'MATCHING',     label: 'Cocokkan Bahasa Batak' },
  { value: 'GUESS_IMAGE',  label: 'Tebak Gambar Budaya' },
  { value: 'WORD_ARRANGE', label: 'Susun Kata Batak' },
  { value: 'MEMORY_CARD',  label: 'Memory Card Budaya' },
  { value: 'CATEGORIZE',   label: 'Kelompokkan Benda Budaya' },
  { value: 'ORDER',        label: 'Susun Urutan' },
  { value: 'FIND_OBJECT',  label: 'Cari Benda Budaya' },
  { value: 'PUZZLE',       label: 'Puzzle Gambar Budaya' },
  { value: 'GUESS_SOUND',  label: 'Tebak Suara Budaya' },
];

const DATA_JSON_TEMPLATES: Record<string, string> = {
  MATCHING: JSON.stringify({
    pairs: [
      { batak: "Horas", indo: "Halo/Selamat" },
      { batak: "Mauliate", indo: "Terima kasih" },
      { batak: "Bagas", indo: "Rumah" }
    ]
  }, null, 2),
  GUESS_IMAGE: JSON.stringify({
    questions: [
      { image: "ulos.png", answer: "Ulos",
        options: ["Ulos","Batik","Songket","Tenun"] }
    ]
  }, null, 2),
  WORD_ARRANGE: JSON.stringify({
    words: [
      { scrambled: "SOLU", answer: "ULOS", hint: "Kain tenun khas Batak" }
    ]
  }, null, 2),
};

const ChallengeModal = ({ challenge, onClose, onSave }: {
  challenge: Challenge | null; onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!challenge;
  const [form, setForm] = useState({
    title:        challenge?.title        ?? '',
    description:  challenge?.description  ?? '',
    game_type:    challenge?.game_type    ?? 'MATCHING',
    reward_xp:    challenge?.reward_xp    ?? 100,
    reward_stars: challenge?.reward_stars ?? 2,
    duration_days: 7,
    data_json:    challenge?.data_json    ?? DATA_JSON_TEMPLATES['MATCHING'],
  });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleGameTypeChange = (type: string) => {
    setForm(f => ({
      ...f, game_type: type,
      data_json: DATA_JSON_TEMPLATES[type] ?? '{}',
    }));
  };

  const handleSave = async () => {
    if (!form.title) { setMsg('Judul wajib diisi.'); return; }
    try { JSON.parse(form.data_json); }
    catch { setMsg('Format Data JSON tidak valid.'); return; }
    setSaving(true);
    try {
      if (isEdit) await api.put(`/admin/challenges/${challenge!.id}`, form);
      else await api.post('/admin/challenges', form);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={{ ...styles.modal, maxWidth: 720 }}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>
            {isEdit ? 'Edit Tantangan' : 'Buat Tantangan Mingguan'}
          </h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}

          <div style={styles.field}>
            <label style={styles.label}>Judul Tantangan *</label>
            <input style={styles.input} value={form.title}
              onChange={e => setForm(f => ({...f, title: e.target.value}))}
              placeholder="Tantangan Minggu Ini: Bahasa Batak"/>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Deskripsi</label>
            <input style={styles.input} value={form.description}
              onChange={e => setForm(f => ({...f, description: e.target.value}))}
              placeholder="Cocokkan kata Batak dengan artinya!"/>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Tipe Mini-Game *</label>
            <select style={styles.input} value={form.game_type}
              onChange={e => handleGameTypeChange(e.target.value)}>
              {GAME_TYPES.map(g => (
                <option key={g.value} value={g.value}>{g.label}</option>
              ))}
            </select>
          </div>

          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Reward XP</label>
              <input style={styles.input} type="number" value={form.reward_xp}
                onChange={e => setForm(f => ({...f, reward_xp: +e.target.value}))}/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Reward Bintang</label>
              <input style={styles.input} type="number" value={form.reward_stars}
                onChange={e => setForm(f => ({...f, reward_stars: +e.target.value}))}/>
            </div>
          </div>

          {!isEdit && (
            <div style={styles.field}>
              <label style={styles.label}>Durasi (hari, default 7)</label>
              <input style={styles.input} type="number" value={form.duration_days}
                onChange={e => setForm(f => ({...f, duration_days: +e.target.value}))}/>
            </div>
          )}

          <div style={styles.field}>
            <label style={styles.label}>Data Konten (JSON) *</label>
            <textarea
              style={{ ...styles.input, height: 160,
                resize: 'vertical', fontFamily: 'monospace', fontSize: '0.78rem' }}
              value={form.data_json}
              onChange={e => setForm(f => ({...f, data_json: e.target.value}))}/>
            <small style={{ opacity: 0.5, fontSize: '0.72rem' }}>
              Format JSON otomatis menyesuaikan tipe game yang dipilih.
              Edit sesuai kebutuhan konten.
            </small>
          </div>
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14}/> {saving ? 'Menyimpan...' : 'Publish Tantangan'}
          </button>
        </div>
      </div>
    </div>
  );
};

const ChallengePage: React.FC = () => {
  const [challenges, setChallenges] = useState<Challenge[]>([]);
  const [loading, setLoading]       = useState(true);
  const [showModal, setShowModal]   = useState(false);
  const [editItem, setEditItem]     = useState<Challenge | null>(null);

  const fetch = () => {
    setLoading(true);
    api.get('/admin/challenges')
      .then(r => setChallenges(r.data || []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleDelete = async (id: number) => {
    if (!confirm('Hapus tantangan ini?')) return;
    await api.delete(`/admin/challenges/${id}`);
    fetch();
  };

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><Trophy size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Tantangan Mingguan</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>
              Kelola mini-game tantangan mingguan untuk pemain
            </p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}>
            <RefreshCw size={14}/> Refresh
          </button>
          <button onClick={() => { setEditItem(null); setShowModal(true); }}
            style={styles.btnPrimary}>
            <Plus size={14}/> Buat Tantangan
          </button>
        </div>
      </div>

      {/* Info */}
      <div className="wood-panel" style={{ borderLeft: '4px solid #D4AF37' }}>
        <p style={{ fontSize: '0.82rem', opacity: 0.8, lineHeight: 1.7 }}>
          <strong style={{ color: '#D4AF37' }}>Cara kerja:</strong> Hanya 1 tantangan
          aktif per minggu. Saat kamu publish tantangan baru, tantangan lama
          otomatis dinonaktifkan. Pemain mendapat notifikasi dan floating button
          muncul di aplikasi.
        </p>
      </div>

      {loading ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 40, opacity: 0.5 }}>
          Memuat...
        </div>
      ) : challenges.length === 0 ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 60, opacity: 0.4 }}>
          <Trophy size={48} style={{ margin: '0 auto 12px', display: 'block' }}/>
          <p>Belum ada tantangan. Buat tantangan pertama!</p>
        </div>
      ) : challenges.map(ch => (
        <div key={ch.id} className="wood-panel">
          <div style={{ display: 'flex', alignItems: 'flex-start', gap: 16 }}>
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10,
                marginBottom: 6 }}>
                <span className={`badge ${ch.is_active ? 'green' : 'orange'}`}>
                  {ch.is_active ? '🟢 AKTIF' : 'Nonaktif'}
                </span>
                <span style={{ fontSize: '0.75rem', opacity: 0.5 }}>
                  {GAME_TYPES.find(g => g.value === ch.game_type)?.label}
                </span>
              </div>
              <div style={{ fontWeight: 'bold', fontSize: '1rem',
                color: '#D4AF37', marginBottom: 4 }}>{ch.title}</div>
              <div style={{ fontSize: '0.78rem', opacity: 0.7,
                marginBottom: 8 }}>{ch.description}</div>
              <div style={{ display: 'flex', gap: 8 }}>
                <span className="badge blue">✨ +{ch.reward_xp} XP</span>
                <span className="badge green">⭐ +{ch.reward_stars}</span>
              </div>
            </div>
            <div style={{ display: 'flex', gap: 8, flexShrink: 0 }}>
              <button onClick={() => { setEditItem(ch); setShowModal(true); }}
                style={{ background: 'none', border: 'none',
                  color: '#D4AF37', cursor: 'pointer' }}>
                <Edit2 size={16}/>
              </button>
              <button onClick={() => handleDelete(ch.id)}
                style={{ background: 'none', border: 'none',
                  color: '#F44336', cursor: 'pointer' }}>
                <Trash2 size={16}/>
              </button>
            </div>
          </div>
        </div>
      ))}

      {showModal && (
        <ChallengeModal
          challenge={editItem}
          onClose={() => setShowModal(false)}
          onSave={() => { setShowModal(false); fetch(); }}/>
      )}
    </div>
  );
};

export default ChallengePage;