// Shared inline styles untuk semua halaman admin
export const styles = {
  overlay: {
    position: 'fixed' as const, inset: 0, background: 'rgba(0,0,0,0.75)',
    display: 'flex', alignItems: 'center', justifyContent: 'center',
    zIndex: 9999, padding: 20,
  },
  modal: {
    background: 'linear-gradient(145deg, #3d2010, #241308)',
    border: '2px solid #D4AF37', borderRadius: 16,
    width: '100%', maxWidth: 680,
    maxHeight: '90vh', display: 'flex', flexDirection: 'column' as const,
    boxShadow: '0 20px 60px rgba(0,0,0,0.8)',
  },
  modalHead: {
    display: 'flex', justifyContent: 'space-between', alignItems: 'center',
    padding: '16px 20px', borderBottom: '1px solid rgba(212,175,55,0.2)',
  },
  modalBody: {
    padding: '20px', overflowY: 'auto' as const, flex: 1,
    display: 'flex', flexDirection: 'column' as const, gap: 12,
  },
  modalFoot: {
    padding: '14px 20px', borderTop: '1px solid rgba(212,175,55,0.2)',
    display: 'flex', gap: 10, justifyContent: 'flex-end',
  },
  field: { display: 'flex', flexDirection: 'column' as const, gap: 5 },
  label: { fontSize: '0.78rem', opacity: 0.7, fontWeight: 600 },
  input: {
    padding: '9px 12px', background: 'rgba(0,0,0,0.4)',
    border: '1px solid #5C3B26', borderRadius: 7, color: '#F5F5DC',
    fontSize: '0.85rem', outline: 'none', width: '100%',
    fontFamily: 'Poppins, sans-serif',
  },
  grid2: { display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 },
  grid4: { display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 10 },
  btnPrimary: {
    display: 'flex', alignItems: 'center', gap: 6,
    padding: '8px 16px', background: 'rgba(77,112,35,0.3)',
    border: '1px solid #4D7023', borderRadius: 8,
    color: '#82ca9d', cursor: 'pointer', fontSize: '0.82rem',
    fontWeight: 600, transition: '0.2s',
  },
  btnSecondary: {
    display: 'flex', alignItems: 'center', gap: 6,
    padding: '8px 14px', background: 'rgba(0,0,0,0.3)',
    border: '1px solid #5C3B26', borderRadius: 8,
    color: '#F5F5DC', cursor: 'pointer', fontSize: '0.82rem',
    transition: '0.2s',
  },
  btnIcon: {
    background: 'rgba(0,0,0,0.2)', border: '1px solid rgba(255,255,255,0.1)',
    borderRadius: 6, color: '#F5F5DC', cursor: 'pointer',
    padding: '6px 8px', display: 'flex', alignItems: 'center',
    transition: '0.2s',
  },
  closeBtn: {
    background: 'none', border: 'none', color: '#F44336',
    cursor: 'pointer', padding: 4, display: 'flex', alignItems: 'center',
  },
  msgBox: {
    padding: '10px 14px', borderRadius: 7, fontSize: '0.82rem',
    background: 'rgba(244,67,54,0.15)', border: '1px solid #F44336', color: '#ff8a80',
  },
};