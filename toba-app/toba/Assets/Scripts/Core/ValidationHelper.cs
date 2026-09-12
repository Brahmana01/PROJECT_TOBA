public static class ValidationHelper
{
    // ── NAMA ──────────────────────────────────────────────
    public static (bool ok, string error) ValidateName(string name)
    {
        name = name?.Trim() ?? "";
        if (name.Length < 2)  return (false, "Nama minimal 2 karakter.");
        if (name.Length > 30) return (false, "Nama maksimal 30 karakter.");
        // Hanya huruf, spasi, titik
        foreach (char c in name)
            if (!char.IsLetter(c) && c != ' ' && c != '.')
                return (false, "Nama hanya boleh berisi huruf dan spasi.");
        return (true, "");
    }

    // ── PIN ───────────────────────────────────────────────
    public static (bool ok, string error) ValidatePin(string pin)
    {
        pin = pin?.Trim() ?? "";
        if (pin.Length != 6) return (false, "PIN harus tepat 6 digit angka.");
        foreach (char c in pin)
            if (!char.IsDigit(c))
                return (false, "PIN hanya boleh berisi angka.");
        return (true, "");
    }

    public static (bool ok, string error) ValidatePinMatch(string pin, string confirm)
    {
        var r = ValidatePin(pin);
        if (!r.ok) return r;
        if (pin != confirm) return (false, "Konfirmasi PIN tidak cocok.");
        return (true, "");
    }

    // ── USERNAME ──────────────────────────────────────────
    public static (bool ok, string error) ValidateUsername(string username)
    {
        username = username?.Trim() ?? "";
        if (username.Length < 3) return (false, "Username minimal 3 karakter.");
        if (username.Length > 20) return (false, "Username maksimal 20 karakter.");
        if (username.Contains(" ")) return (false, "Username tidak boleh ada spasi.");
        return (true, "");
    }

    // ── ROOM CODE ─────────────────────────────────────────
    public static (bool ok, string error) ValidateRoomCode(string code)
    {
        code = code?.Trim().ToUpper() ?? "";
        if (code.Length < 4) return (false, "Kode room minimal 4 karakter.");
        if (code.Length > 8) return (false, "Kode room tidak valid.");
        return (true, "");
    }
}