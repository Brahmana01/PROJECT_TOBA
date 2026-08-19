import React, { useEffect, useState } from 'react';
import { Sword, Plus, Edit2, Trash2, RefreshCw, X, Check } from 'lucide-react';
import { adminGetBattleQuestions, adminCreateBattleQuestion, adminUpdateBattleQuestion, adminDeleteBattleQuestion } from '../services/api';
import type { BattleQuestion, QuestionOption } from '../types';
import { styles } from '../styles/shared';

const BattleQuestionModal = ({ question, onClose, onSave }: {
  question: BattleQuestion | null; onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!question;
  const [q, setQ]     = useState(question?.question ?? '');
  const [exp, setExp] = useState(question?.explanation ?? '');
  const [opts, setOpts] = useState<QuestionOption[]>(
    question?.options ?? [
      { option_text: '', is_correct: true  },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
    ]
  );
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSave = async () => {
    if (!q || !exp) { setMsg('Pertanyaan dan penjelasan wajib.'); return; }
    const valid = opts.filter(o => o.option_text.trim());
    if (valid.length < 2) { setMsg('Minimal 2 pilihan.'); return; }
    if (!valid.some(o => o.is_correct)) { setMsg('Pilih 1 jawaban benar.'); return; }
    setSaving(true);
    try {
      const payload = { question: q, explanation: exp, is_active: true, options: valid };
      if (isEdit) await adminUpdateBattleQuestion(question!.id, payload);
      else await adminCreateBattleQuestion(payload);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={{...styles.modal, maxWidth: 600}}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Soal Battle' : 'Tambah Soal Battle'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.field}>
            <label style={styles.label}>Pertanyaan *</label>
            <textarea style={{...styles.input, height: 70, resize: 'vertical'}} value={q} onChange={e => setQ(e.target.value)} placeholder="Tulis pertanyaan..."/>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Penjelasan *</label>
            <textarea style={{...styles.input, height: 60, resize: 'vertical'}} value={exp} onChange={e => setExp(e.target.value)} placeholder="Penjelasan jawaban..."/>
          </div>
          <label style={styles.label}>Pilihan Jawaban</label>
          {opts.map((opt, i) => (
            <div key={i} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
              <span style={{ color: '#D4AF37', fontWeight: 'bold', width: 20 }}>{String.fromCharCode(65+i)}.</span>
              <input type="radio" name="correct" checked={opt.is_correct} onChange={() => setOpts(p => p.map((o, idx) => ({...o, is_correct: idx === i})))} style={{ accentColor: '#4CAF50' }}/>
              <input style={{...styles.input, flex: 1, margin: 0}} value={opt.option_text} onChange={e => setOpts(p => p.map((o, idx) => idx === i ? {...o, option_text: e.target.value} : o))} placeholder={`Pilihan ${String.fromCharCode(65+i)}`}/>
              {opt.is_correct && <span style={{ color: '#4CAF50', fontSize: 12, whiteSpace: 'nowrap' }}>✓ Benar</span>}
            </div>
          ))}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}><Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}</button>
        </div>
      </div>
    </div>
  );
};

const BattlePage: React.FC = () => {
  const [questions, setQuestions] = useState<BattleQuestion[]>([]);
  const [loading, setLoading]     = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editQ, setEditQ]         = useState<BattleQuestion | null>(null);
  const [search, setSearch]       = useState('');

  const fetch = () => {
    setLoading(true);
    adminGetBattleQuestions().then(r => setQuestions(r.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleDelete = async (id: number) => {
    if (!confirm('Hapus soal ini?')) return;
    await adminDeleteBattleQuestion(id);
    fetch();
  };

  const filtered = questions.filter(q =>
    q.question.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><Sword size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Bank Soal Battle Quiz</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>{questions.length} soal tersedia</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}><RefreshCw size={14}/> Refresh</button>
          <button onClick={() => { setEditQ(null); setShowModal(true); }} style={styles.btnPrimary}>
            <Plus size={14}/> Tambah Soal
          </button>
        </div>
      </div>

      <div className="wood-panel">
        <div style={{ display: 'flex', gap: 12, marginBottom: 20, alignItems: 'center' }}>
          <input
            value={search} onChange={e => setSearch(e.target.value)}
            style={{...styles.input, flex: 1, margin: 0}} placeholder="Cari soal..."
          />
          <span style={{ fontSize: '0.8rem', opacity: 0.5 }}>{filtered.length} soal</span>
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', padding: 40, opacity: 0.5 }}>Memuat...</div>
        ) : filtered.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 40, opacity: 0.4 }}>
            <Sword size={48} style={{ margin: '0 auto 12px', display: 'block' }}/>
            <p>Belum ada soal battle</p>
          </div>
        ) : filtered.map((q, i) => (
          <div key={q.id} style={{ marginBottom: 10, background: 'rgba(0,0,0,0.2)', borderRadius: 8, border: '1px solid rgba(212,175,55,0.1)', overflow: 'hidden' }}>
            <div style={{ padding: '12px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 }}>
              <div style={{ flex: 1 }}>
                <span style={{ fontSize: '0.7rem', color: '#D4AF37', marginRight: 8 }}>#{i+1}</span>
                <span style={{ fontSize: '0.85rem' }}>{q.question}</span>
              </div>
              <div style={{ display: 'flex', gap: 4, flexShrink: 0, alignItems: 'center' }}>
                <span className={`badge ${q.is_active ? 'green' : 'orange'}`} style={{ fontSize: 9 }}>{q.is_active ? 'Aktif' : 'Nonaktif'}</span>
                <button onClick={() => { setEditQ(q); setShowModal(true); }} style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}><Edit2 size={14}/></button>
                <button onClick={() => handleDelete(q.id)} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}><Trash2 size={14}/></button>
              </div>
            </div>
            <div style={{ padding: '0 16px 12px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6 }}>
              {q.options?.map((opt, oi) => (
                <div key={oi} style={{ padding: '5px 10px', borderRadius: 5, fontSize: '0.78rem', background: opt.is_correct ? 'rgba(76,175,80,0.15)' : 'rgba(0,0,0,0.2)', border: opt.is_correct ? '1px solid #4CAF50' : '1px solid rgba(255,255,255,0.05)' }}>
                  {String.fromCharCode(65+oi)}. {opt.option_text}
                  {opt.is_correct && <span style={{ color: '#4CAF50', marginLeft: 4 }}>✓</span>}
                </div>
              ))}
            </div>
            <div style={{ padding: '0 16px 12px', fontSize: '0.75rem', opacity: 0.7, borderLeft: '3px solid #D4AF37', marginLeft: 16, marginRight: 16, paddingLeft: 10 }}>
              💡 {q.explanation}
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <BattleQuestionModal question={editQ} onClose={() => setShowModal(false)} onSave={() => { setShowModal(false); fetch(); }}/>
      )}
    </div>
  );
};

export default BattlePage;