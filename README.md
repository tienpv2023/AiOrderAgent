# AiOrderAgent — .NET AI Agent & RAG System

## 1. Giới thiệu dự án

**AiOrderAgent** là dự án ứng dụng AI được xây dựng trên nền tảng .NET, tích hợp Large Language Model (LLM), PostgreSQL và pgvector để tìm kiếm thông tin, truy xuất tài liệu liên quan và tạo câu trả lời dựa trên dữ liệu.

Dự án tập trung vào việc xây dựng một hệ thống AI có khả năng kết hợp truy xuất dữ liệu với mô hình ngôn ngữ lớn, hạn chế việc tạo thông tin không có căn cứ và cung cấp nguồn tham chiếu cho câu trả lời.

Đây là dự án thực hành nhằm áp dụng các kỹ thuật AI vào kiến trúc ứng dụng .NET thực tế, đồng thời thể hiện kiến thức về API, cơ sở dữ liệu, xử lý bất đồng bộ và thiết kế hệ thống.

## 2. Công nghệ sử dụng

- **Backend:** C#, .NET / ASP.NET Core Web API
- **AI / LLM:** OpenAI API
- **Text Embedding:** OpenAI `text-embedding-3-small`
- **Chat Model:** OpenAI `gpt-4o-mini`
- **Database:** PostgreSQL
- **Vector Database:** pgvector
- **Database Access:** Npgsql
- **Search:** PostgreSQL Full-Text Search, Vector Search, Hybrid Search
- **Ranking:** Reciprocal Rank Fusion (RRF)
- **API Testing:** Swagger / OpenAPI

## 3. Các chức năng chính

### 3.1. AI Agent với Function Calling

Xây dựng AI Agent có khả năng gọi function để truy xuất dữ liệu nghiệp vụ thay vì tự tạo câu trả lời từ dữ liệu không được xác minh.

Luồng xử lý:

1. Người dùng gửi câu hỏi.
2. LLM phân tích yêu cầu.
3. Agent gọi function phù hợp, ví dụ `GetOrderStatus`.
4. Function truy vấn dữ liệu từ hệ thống.
5. Kết quả được trả lại cho LLM.
6. LLM tạo câu trả lời dựa trên kết quả thực tế.

### 3.2. Retrieval-Augmented Generation (RAG)

Triển khai pipeline RAG để tìm kiếm thông tin liên quan trước khi gửi ngữ cảnh cho LLM.

Pipeline:

`Question → Embedding → Retrieval → Context Building → LLM → Answer`

Các bước chính:

- Chuyển câu hỏi thành vector embedding.
- Tìm kiếm các đoạn tài liệu liên quan trong PostgreSQL/pgvector.
- Lấy các đoạn tài liệu phù hợp làm ngữ cảnh.
- Gửi câu hỏi và ngữ cảnh đến LLM.
- Tạo câu trả lời dựa trên thông tin truy xuất được.

### 3.3. Hybrid Search

Kết hợp hai phương pháp tìm kiếm để cải thiện khả năng truy xuất tài liệu:

- **Keyword Search:** Tìm kiếm dựa trên từ khóa bằng PostgreSQL Full-Text Search.
- **Vector Search:** Tìm kiếm theo độ tương đồng ngữ nghĩa bằng pgvector.
- **Reciprocal Rank Fusion (RRF):** Kết hợp thứ hạng từ hai danh sách kết quả.
- **Reranking:** Sắp xếp lại các tài liệu ứng viên trước khi xây dựng ngữ cảnh.

Luồng xử lý:

`Question → Query Processing → Keyword Search + Vector Search → RRF → Security Filter → Reranking → Top 5 Documents`

### 3.4. Document Ingestion và Embedding

Xây dựng quy trình nạp dữ liệu và chuẩn bị dữ liệu cho RAG:

- Lưu trữ các đoạn tài liệu trong bảng `document_chunks`.
- Tạo vector embedding cho nội dung tài liệu.
- Lưu vector vào PostgreSQL bằng pgvector.
- Truy xuất các tài liệu đã có embedding để phục vụ tìm kiếm.

### 3.5. Security và kiểm soát ngữ cảnh

Thiết kế bước lọc tài liệu trước khi đưa dữ liệu vào LLM nhằm hạn chế truy xuất tài liệu không được phép sử dụng.

Trong môi trường nhiều người dùng hoặc nhiều tenant, bộ lọc cần được kết hợp với danh tính đã xác thực và điều kiện phân quyền ngay tại tầng truy vấn dữ liệu.

## 4. Kiến trúc hệ thống

```text
                    User
                      |
                      v
                 ASP.NET Core
                   Web API
                      |
             +--------+--------+
             |                 |
             v                 v
         AI Agent          RAG Pipeline
             |                 |
             v          +------+------+
       Function Calling |             |
             |           v             v
             v      Keyword Search  Vector Search
       Business DB       |             |
                         +------+------+
                                |
                                v
                         Hybrid Search
                                |
                                v
                              RRF
                                |
                                v
                         Security Filter
                                |
                                v
                            Reranker
                                |
                                v
                          Context Builder
                                |
                                v
                           OpenAI LLM
                                |
                                v
                         Answer + Sources
```

## 5. Database Design

Bảng `document_chunks` lưu trữ các đoạn tài liệu phục vụ tìm kiếm:

| Column | Description |
|---|---|
| `id` | Định danh duy nhất của đoạn tài liệu |
| `document_name` | Tên tài liệu nguồn |
| `content` | Nội dung đoạn tài liệu |
| `embedding` | Vector embedding của nội dung |
| `created_at` | Thời điểm tạo bản ghi |
| `tenant_id` | Mã tenant dùng cho phân tách dữ liệu, nếu được triển khai |

pgvector hỗ trợ lưu trữ vector và thực hiện tìm kiếm tương đồng. Chỉ mục HNSW có thể được sử dụng để tối ưu tìm kiếm vector trên tập dữ liệu lớn.

## 6. Cấu trúc dự án

Cấu trúc tham khảo theo các thành phần của hệ thống:

```text
AiOrderAgent/
├── Controllers/
│   ├── ChatController.cs
│   └── RagController.cs
├── Services/
│   ├── AiAgentService.cs
│   ├── EmbeddingService.cs
│   ├── DocumentIngestionService.cs
│   ├── PostgresVectorStore.cs
│   ├── VectorSearchService.cs
│   ├── HybridSearchService.cs
│   ├── DocumentSecurityFilter.cs
│   ├── DocumentReranker.cs
│   ├── ContextBuilder.cs
│   ├── LlmService.cs
│   └── RagAnswerService.cs
├── Tools/
│   └── GetOrderStatusTool.cs
├── Program.cs
├── appsettings.json
└── README.md
```

Tên file và cấu trúc có thể thay đổi theo phiên bản hiện tại của dự án.

## 7. Cài đặt và chạy dự án

### Yêu cầu

- .NET SDK tương thích với target framework của dự án.
- PostgreSQL.
- pgvector đã được cài đặt và kích hoạt trong database.
- OpenAI API key.

### Bước 1: Cấu hình database

Cập nhật connection string PostgreSQL trong cấu hình ứng dụng hoặc User Secrets.

### Bước 2: Cấu hình OpenAI API key

Thiết lập biến môi trường `OPENAI_API_KEY`. Không commit API key vào Git hoặc đưa key thật vào README.

### Bước 3: Build dự án

```bash
dotnet restore
dotnet build
```

### Bước 4: Chạy ứng dụng

```bash
dotnet run
```

Mở Swagger theo URL được hiển thị trong console để kiểm tra các API.

## 8. API tiêu biểu

### RAG Ask

`POST /api/rag/ask`

Request:

```json
{
  "question": "Đơn hàng ORD-1024 đang ở đâu?"
}
```

Response minh họa:

```json
{
  "answer": "Thông tin trả lời dựa trên các tài liệu được truy xuất."
}
```

Response thực tế phụ thuộc vào dữ liệu trong database và cấu trúc response hiện được triển khai.

## 9. Những kiến thức kỹ thuật thể hiện qua dự án

Dự án giúp thực hành các chủ đề quan trọng khi phát triển hệ thống .NET tích hợp AI:

- Thiết kế RESTful API bằng ASP.NET Core.
- Dependency Injection và phân chia trách nhiệm giữa các service.
- Xử lý I/O bất đồng bộ bằng `async/await` và `CancellationToken`.
- Tích hợp OpenAI API cho embedding và sinh câu trả lời.
- Thiết kế pipeline Retrieval-Augmented Generation.
- Tìm kiếm kết hợp Keyword Search và Vector Search.
- Hợp nhất thứ hạng bằng Reciprocal Rank Fusion.
- Truy vấn PostgreSQL và sử dụng pgvector.
- Thiết kế bước kiểm soát tài liệu và xây dựng ngữ cảnh cho LLM.
- Xử lý lỗi, cấu hình và kiểm thử API.

## 10. Hướng phát triển

- Hoàn thiện cơ chế trả về nguồn tham chiếu trong response.
- Bổ sung xử lý lỗi API, timeout, rate limit và retry có kiểm soát.
- Tăng chất lượng phân đoạn tài liệu và tìm kiếm tiếng Việt.
- Thay thế reranker từ khóa bằng semantic reranker phù hợp.
- Triển khai phân quyền và tenant isolation ở tầng truy vấn database.
- Bổ sung unit test, integration test và đánh giá chất lượng retrieval.
- Thêm logging, monitoring và đo độ trễ từng bước trong pipeline.

## 11. Mục tiêu dự án

AiOrderAgent hướng đến việc xây dựng một ứng dụng AI có thể tích hợp vào hệ thống nghiệp vụ thực tế, kết hợp thế mạnh của .NET backend với các kỹ thuật LLM, RAG và Hybrid Search.

Dự án được sử dụng làm portfolio để trình bày năng lực thiết kế backend, tích hợp AI và giải quyết các vấn đề kỹ thuật trong quá trình phát triển ứng dụng.
