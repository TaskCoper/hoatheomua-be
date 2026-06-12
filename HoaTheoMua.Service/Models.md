                        ┌──────────────────────┐
                        │ ApiResponse (Abstract)│ <─── Lớp cha trừu tượng (Chứa metadata)
                        └──────────┬───────────┘
                        │
                        ┌──────────────┴──────────────┐
                        ▼                             ▼
                        ┌────────────────────┐          ┌──────────────────────────────┐
                        │    BaseResponse    │          │    BasePaginationResponse    │
                        ├────────────────────┤          ├──────────────────────────────┤
                        │ public object Value│          │ public PaginationValue Value │
                        └────────────────────┘          └──────────────┬───────────────┘
                                                                       │ (Chứa thông tin)
                                                                       ▼
                                                            ┌───────────────────┐
                                                            │  PaginationValue  │
                                                            ├───────────────────┤
                                                            │ List<object> Items│
                                                            │ PageIndex, ...    │
                                                            └───────────────────┘