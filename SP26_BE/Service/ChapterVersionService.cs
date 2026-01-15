using Repository;
using Repository.Models;

namespace Service
{
    public class ChapterVersionService
    {
        private readonly ChapterVersionRepository _versionRepository;
        private readonly ChapterRepository _chapterRepository;

        public ChapterVersionService()
        {
            _versionRepository = new ChapterVersionRepository();
            _chapterRepository = new ChapterRepository();
        }

        public async Task<(bool Success, string Message, ChapterVersion? Version)> CreateVersionAsync(
            int userId,
            int chapterId,
            string rawContent)
        {
            // Kiểm tra chapter có tồn tại và user có quyền không
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương", null);
            }

            if (chapter.Project?.AuthorId != userId)
            {
                return (false, "Bạn không có quyền thêm phiên bản cho chương này", null);
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return (false, "Nội dung không được để trống", null);
            }

            // Lấy version number tiếp theo
            var nextVersionNumber = await _versionRepository.GetMaxVersionNumberAsync(chapterId) + 1;

            // Tính số từ
            var wordCount = await _versionRepository.CalculateWordCountAsync(rawContent);

            var newVersion = new ChapterVersion
            {
                ChapterId = chapterId,
                VersionNumber = nextVersionNumber,
                RawContent = rawContent.Trim(),
                WordCount = wordCount,
                UploadDate = DateTime.UtcNow,
                IsActive = nextVersionNumber == 1 // Phiên bản đầu tiên sẽ active mặc định
            };

            // Nếu là version mới nhất, deactivate các version cũ
            if (nextVersionNumber > 1)
            {
                // Version mới không tự động active, user phải chọn
                newVersion.IsActive = false;
            }

            var createdVersion = await _versionRepository.CreateAsync(newVersion);
            return (true, "Tạo phiên bản thành công", createdVersion);
        }

        public async Task<(bool Success, string Message, ChapterVersion? Version)> GetVersionByIdAsync(int versionId)
        {
            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null)
            {
                return (false, "Không tìm thấy phiên bản", null);
            }

            return (true, "Lấy thông tin phiên bản thành công", version);
        }

        public async Task<(bool Success, string Message, List<ChapterVersion>? Versions)> GetVersionsByChapterIdAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương", null);
            }

            var versions = await _versionRepository.GetByChapterIdAsync(chapterId);
            return (true, "Lấy danh sách phiên bản thành công", versions);
        }

        public async Task<(bool Success, string Message, ChapterVersion? Version)> GetActiveVersionAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương", null);
            }

            var activeVersion = await _versionRepository.GetActiveVersionByChapterIdAsync(chapterId);
            if (activeVersion == null)
            {
                return (false, "Chương này chưa có phiên bản nào active", null);
            }

            return (true, "Lấy phiên bản active thành công", activeVersion);
        }

        public async Task<(bool Success, string Message, ChapterVersion? Version)> UpdateVersionAsync(
            int userId,
            int versionId,
            string rawContent)
        {
            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null)
            {
                return (false, "Không tìm thấy phiên bản", null);
            }

            // Kiểm tra quyền sở hữu
            if (!await _versionRepository.IsOwnerAsync(versionId, userId))
            {
                return (false, "Bạn không có quyền chỉnh sửa phiên bản này", null);
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return (false, "Nội dung không được để trống", null);
            }

            version.RawContent = rawContent.Trim();
            version.WordCount = await _versionRepository.CalculateWordCountAsync(rawContent);

            var updatedVersion = await _versionRepository.UpdateAsync(version);
            return (true, "Cập nhật phiên bản thành công", updatedVersion);
        }

        public async Task<(bool Success, string Message)> DeleteVersionAsync(int userId, int versionId)
        {
            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null)
            {
                return (false, "Không tìm thấy phiên bản");
            }

            // Kiểm tra quyền sở hữu
            if (!await _versionRepository.IsOwnerAsync(versionId, userId))
            {
                return (false, "Bạn không có quyền xóa phiên bản này");
            }

            // Không cho xóa version đang active
            if (version.IsActive == true)
            {
                return (false, "Không thể xóa phiên bản đang active. Vui lòng chọn phiên bản khác làm active trước");
            }

            var success = await _versionRepository.DeleteAsync(versionId);
            if (!success)
            {
                return (false, "Xóa phiên bản thất bại");
            }

            return (true, "Xóa phiên bản thành công");
        }

        public async Task<(bool Success, string Message)> SetActiveVersionAsync(int userId, int versionId)
        {
            var version = await _versionRepository.GetByIdAsync(versionId);
            if (version == null)
            {
                return (false, "Không tìm thấy phiên bản");
            }

            // Kiểm tra quyền sở hữu
            if (!await _versionRepository.IsOwnerAsync(versionId, userId))
            {
                return (false, "Bạn không có quyền thay đổi phiên bản active");
            }

            var success = await _versionRepository.SetActiveVersionAsync(versionId);
            if (!success)
            {
                return (false, "Đặt phiên bản active thất bại");
            }

            return (true, $"Đã đặt phiên bản {version.VersionNumber} làm active");
        }
    }
}