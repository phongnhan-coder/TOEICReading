(function () {
  "use strict";

  var workspace = document.getElementById("toeicWorkspace");
  if (!workspace) {
    return;
  }

  var adminWorkspace = document.getElementById("adminWorkspace");
  var studentWorkspace = document.getElementById("studentWorkspace");
  var modeSwitch = document.getElementById("modeSwitch");
  var studentScreens = Array.prototype.slice.call(
    studentWorkspace.querySelectorAll("[data-student-screen]"),
  );
  var answers = {};
  var markedQuestions = {};
  var currentQuestion = 149;
  var questionRange = [147, 200];
  var remainingSeconds = 75 * 60;
  var timerHandle = null;
  var noticeTimeout = null;
  var selectedDocx = null;
  var importedPreview = null;
  var savedExam = null;

  function showMode(mode) {
    workspace.dataset.mode = mode;
    var isStudent = mode === "student";
    adminWorkspace.hidden = isStudent;
    studentWorkspace.hidden = !isStudent;
    modeSwitch.textContent = isStudent
      ? "Chuyển sang quản trị"
      : "Chuyển sang học viên";
    if (isStudent) {
      showStudentScreen("list");
    }
  }

  function showStudentScreen(name) {
    studentScreens.forEach(function (screen) {
      screen.hidden = screen.dataset.studentScreen !== name;
    });
    studentWorkspace.querySelectorAll("[data-student-nav]").forEach(function (button) {
      button.classList.toggle(
        "active",
        button.dataset.studentNav === name ||
          (name === "detail" && button.dataset.studentNav === "list"),
      );
    });
    var bottomNav = studentWorkspace.querySelector(".student-bottom-nav");
    if (bottomNav) {
      bottomNav.hidden = name === "exam";
    }
  }

  function showNotice(element, message, kind) {
    if (!element) {
      return;
    }

    element.textContent = message;
    element.classList.remove("error", "success", "info");
    element.classList.add(kind || "info", "visible");
    if (noticeTimeout) {
      window.clearTimeout(noticeTimeout);
    }
    noticeTimeout = window.setTimeout(function () {
      element.classList.remove("visible");
    }, 6000);
  }

  function renderQuestionGrid() {
    var grid = document.getElementById("questionGrid");
    grid.replaceChildren();
    for (var number = questionRange[0]; number <= questionRange[1]; number += 1) {
      var button = document.createElement("button");
      button.type = "button";
      button.className = "test-question-number";
      button.textContent = number;
      button.dataset.question = number;
      button.setAttribute("aria-label", "Câu " + number);
      if (number === currentQuestion) {
        button.classList.add("current");
        button.setAttribute("aria-current", "true");
      }
      if (answers[number]) {
        button.classList.add("answered");
      }
      if (markedQuestions[number]) {
        button.classList.add("marked");
        button.title = "Đã đánh dấu";
      }
      grid.appendChild(button);
    }
    document.getElementById("questionIndex").textContent = currentQuestion;
  }

  function setCurrentQuestion(number) {
    currentQuestion = Math.max(
      questionRange[0],
      Math.min(questionRange[1], number),
    );
    var savedAnswer = answers[currentQuestion];
    document.querySelectorAll('input[name="answer"]').forEach(function (input) {
      input.checked = input.value === savedAnswer;
    });
    var markButton = document.getElementById("markQuestion");
    if (markButton) {
      markButton.classList.toggle("toeic-btn-soft", Boolean(markedQuestions[currentQuestion]));
      markButton.innerHTML = markedQuestions[currentQuestion]
        ? '<i class="fas fa-bookmark" aria-hidden="true"></i> Đã đánh dấu'
        : "Đánh dấu";
    }
    renderQuestionGrid();
  }

  function startTimer() {
    if (timerHandle) {
      return;
    }
    timerHandle = window.setInterval(function () {
      remainingSeconds = Math.max(0, remainingSeconds - 1);
      var minutes = Math.floor(remainingSeconds / 60);
      var seconds = remainingSeconds % 60;
      document.getElementById("examTimer").textContent =
        String(minutes).padStart(2, "0") + ":" + String(seconds).padStart(2, "0");
      if (remainingSeconds === 0) {
        window.clearInterval(timerHandle);
        timerHandle = null;
        showStudentScreen("result");
      }
    }, 1000);
  }

  function applyExamFilters() {
    var term = document.getElementById("examSearch").value.trim().toLocaleLowerCase();
    var status = document.getElementById("examStatus").value;
    document.querySelectorAll("[data-exam-row]").forEach(function (row) {
      var matchesTerm = row.textContent.toLocaleLowerCase().includes(term);
      var matchesStatus = !status || row.dataset.status === status;
      row.hidden = !matchesTerm || !matchesStatus;
    });
  }

  function formatBytes(bytes) {
    return bytes < 1024 * 1024
      ? (bytes / 1024).toFixed(1) + " KB"
      : (bytes / (1024 * 1024)).toFixed(2) + " MB";
  }

  function setDocxStatus(text, kind) {
    var status = document.getElementById("uploadStatus");
    status.textContent = text;
    status.classList.remove(
      "toeic-status-draft",
      "docx-status-uploading",
      "docx-status-uploaded",
      "docx-status-error",
    );
    status.classList.add(kind);
  }

  function resetImportedPreview() {
    importedPreview = null;
    savedExam = null;
    setEditorReadOnly(false);
    document.getElementById("previewQuestionList").replaceChildren();
    document.getElementById("previewEmptyState").hidden = false;
    document.getElementById("examQuestionCount").value = "0";
    document.getElementById("saveDraft").disabled = true;
    document.getElementById("publishExam").disabled = true;
    document.getElementById("editorNotice").classList.remove("visible");
  }

  function selectDocx(file) {
    var notice = document.getElementById("importNotice");
    notice.classList.remove("visible");
    if (!file) {
      selectedDocx = null;
      return;
    }

    if (!file.name.toLocaleLowerCase().endsWith(".docx")) {
      selectedDocx = null;
      document.getElementById("examFile").value = "";
      document.getElementById("docxFileRow").hidden = true;
      document.getElementById("docxDropzone").hidden = false;
      document.getElementById("importDocx").disabled = true;
      document.getElementById("cancelDocx").disabled = true;
      resetImportedPreview();
      showNotice(notice, "Định dạng không hợp lệ. Vui lòng chọn file Word .docx.", "error");
      return;
    }

    if (file.size === 0) {
      clearInvalidSelection();
      showNotice(notice, "Tệp đã chọn rỗng. Hãy chọn một file .docx có nội dung.", "error");
      return;
    }

    if (file.size > 25 * 1024 * 1024) {
      clearInvalidSelection();
      showNotice(notice, "Dung lượng tệp vượt giới hạn 25 MB.", "error");
      return;
    }

    selectedDocx = file;
    resetImportedPreview();
    document.getElementById("docxDropzone").hidden = true;
    document.getElementById("docxFileRow").hidden = false;
    document.getElementById("fileName").textContent = file.name;
    document.getElementById("fileSize").textContent = formatBytes(file.size);
    setDocxStatus("Đã chọn · chưa tải lên", "toeic-status-draft");
    document.getElementById("importDocx").disabled = false;
    document.getElementById("cancelDocx").disabled = false;
  }

  function clearInvalidSelection() {
    selectedDocx = null;
    document.getElementById("examFile").value = "";
    document.getElementById("docxFileRow").hidden = true;
    document.getElementById("docxDropzone").hidden = false;
    document.getElementById("importDocx").disabled = true;
    document.getElementById("cancelDocx").disabled = true;
    resetImportedPreview();
  }

  function createEditorField(labelText, value, field, multiline) {
    var wrapper = document.createElement("div");
    wrapper.className = "toeic-form-field" + (multiline ? " full" : "");
    var label = document.createElement("label");
    label.textContent = labelText;
    var control = document.createElement(multiline ? "textarea" : "input");
    if (!multiline) {
      control.type = "text";
    }
    if (field === "number") {
      control.type = "number";
      control.min = "1";
      control.max = "300";
      control.required = true;
    }
    control.value = value || "";
    control.dataset.questionField = field;
    if (["questionText"].includes(field)) {
      control.maxLength = 4000;
    } else if (field.startsWith("option")) {
      control.maxLength = 2000;
    } else if (field === "passage") {
      control.maxLength = 20000;
    }
    label.appendChild(control);
    wrapper.appendChild(label);
    return wrapper;
  }

  function createPartField(part) {
    var wrapper = document.createElement("div");
    wrapper.className = "toeic-form-field";
    var label = document.createElement("label");
    label.textContent = "Part";
    var select = document.createElement("select");
    select.dataset.questionField = "part";
    [5, 6, 7].forEach(function (number) {
      var option = document.createElement("option");
      option.value = number;
      option.textContent = "Part " + number;
      option.selected = number === part;
      select.appendChild(option);
    });
    label.appendChild(select);
    wrapper.appendChild(label);
    return wrapper;
  }

  function createAnswerField(answer) {
    var wrapper = document.createElement("div");
    wrapper.className = "toeic-form-field";
    var label = document.createElement("label");
    label.textContent = "Đáp án đúng (nếu có)";
    var select = document.createElement("select");
    select.dataset.questionField = "correctAnswer";
    [
      ["", "Chưa xác định"],
      ["A", "A"],
      ["B", "B"],
      ["C", "C"],
      ["D", "D"],
    ].forEach(function (item) {
      var option = document.createElement("option");
      option.value = item[0];
      option.textContent = item[1];
      option.selected = item[0] === (answer || "");
      select.appendChild(option);
    });
    label.appendChild(select);
    wrapper.appendChild(label);
    return wrapper;
  }

  function renderImportedQuestions(preview) {
    var list = document.getElementById("previewQuestionList");
    list.replaceChildren();
    preview.questions.forEach(function (question) {
      appendQuestionCard(list, question);
    });

    document.getElementById("previewEmptyState").hidden = preview.questions.length > 0;
    updateQuestionSummary();
  }

  function appendQuestionCard(list, question) {
    var card = document.createElement("section");
    card.className = "toeic-imported-question";
    card.dataset.importedQuestion = "";
    card.questionImages = question.images || [];

    var heading = document.createElement("div");
    heading.className = "toeic-imported-question-head";
    var title = document.createElement("strong");
    title.dataset.questionHeading = "";
    title.textContent = "Câu " + question.number;
    var part = document.createElement("span");
    part.className = "toeic-status toeic-status-live";
    part.dataset.questionPart = "";
    part.textContent = "Part " + question.part;
    var remove = document.createElement("button");
    remove.type = "button";
    remove.className = "toeic-btn";
    remove.dataset.deleteQuestion = "";
    remove.textContent = "Xóa câu";
    heading.append(title, part, remove);

    var fields = document.createElement("div");
    fields.className = "toeic-imported-question-grid";
    fields.appendChild(createPartField(question.part));
    fields.appendChild(createEditorField("Số câu", String(question.number), "number", false));
    fields.appendChild(createEditorField("Nội dung câu hỏi", question.questionText, "questionText", true));
    fields.appendChild(createEditorField("Đoạn văn / ngữ cảnh", question.passage, "passage", true));
    fields.appendChild(createEditorField("Lựa chọn A", question.optionA, "optionA", false));
    fields.appendChild(createEditorField("Lựa chọn B", question.optionB, "optionB", false));
    fields.appendChild(createEditorField("Lựa chọn C", question.optionC, "optionC", false));
    fields.appendChild(createEditorField("Lựa chọn D", question.optionD, "optionD", false));
    fields.appendChild(createAnswerField(question.correctAnswer));
    card.append(heading, fields);

    if (card.questionImages.length) {
      var images = document.createElement("div");
      images.className = "toeic-question-images";
      card.questionImages.forEach(function (image) {
        if (!image || !image.contentType || !image.data) {
          return;
        }
        if (image.contentType.toLowerCase() === "image/tiff") {
          var imageNote = document.createElement("span");
          imageNote.className = "toeic-subtext";
          imageNote.textContent = "Có ảnh TIFF đính kèm (không hỗ trợ xem trực tiếp trong trình duyệt).";
          images.appendChild(imageNote);
          return;
        }
        var imagePreview = document.createElement("img");
        imagePreview.alt = "Ảnh trong câu hỏi " + question.number;
        imagePreview.src = "data:" + image.contentType + ";base64," + image.data;
        images.appendChild(imagePreview);
      });
      if (images.childElementCount) {
        card.appendChild(images);
      }
    }

    list.appendChild(card);
    return card;
  }

  function setEditorReadOnly(readOnly) {
    document.querySelectorAll("#examEditorForm input:not([name='__RequestVerificationToken'])").forEach(function (field) {
      field.disabled = readOnly;
    });
    document.querySelectorAll("#previewQuestionList [data-question-field], #previewQuestionList [data-delete-question]").forEach(function (field) {
      field.disabled = readOnly;
    });
    document.getElementById("addQuestion").disabled = readOnly || !importedPreview;
    document.getElementById("saveDraft").disabled = readOnly || !importedPreview;
  }

  function updatePublishButton() {
    var published = savedExam && savedExam.status === "Published";
    document.getElementById("publishExam").disabled = !savedExam || published;
    document.getElementById("publishExamLabel").textContent =
      published ? "Đã xuất bản" : "Xuất bản đề thi";
  }

  function updateQuestionSummary() {
    var cards = Array.prototype.slice.call(document.querySelectorAll("[data-imported-question]"));
    var counts = { 5: 0, 6: 0, 7: 0 };
    cards.forEach(function (card, index) {
      var partField = card.querySelector('[data-question-field="part"]');
      var numberField = card.querySelector('[data-question-field="number"]');
      var part = Number(partField.value);
      if (counts[part] !== undefined) {
        counts[part] += 1;
      }
      card.querySelector("[data-question-heading]").textContent =
        "Câu " + (numberField.value || index + 1);
      card.querySelector("[data-question-part]").textContent = "Part " + part;
    });

    [5, 6, 7].forEach(function (part) {
      document.querySelectorAll('[data-part-count="' + part + '"]').forEach(function (element) {
        element.textContent = counts[part];
      });
      var state = document.querySelector('[data-part-state="' + part + '"]');
      if (state) {
        state.textContent = counts[part] ? "Có nội dung" : "Chưa có";
        state.classList.toggle("toeic-status-live", counts[part] > 0);
        state.classList.toggle("toeic-status-draft", counts[part] === 0);
      }
    });

    var filledParts = [5, 6, 7].filter(function (part) { return counts[part] > 0; }).length;
    document.getElementById("configuredPartsCount").textContent = filledParts;
    document.getElementById("previewQuestionCount").textContent = cards.length;
    document.getElementById("examQuestionCount").value = cards.length;
    document.getElementById("totalQuestionCount").textContent = cards.length;
    document.getElementById("previewEmptyState").hidden = cards.length > 0;
    document.getElementById("summaryDuration").textContent =
      document.getElementById("examDuration").value || "0";
    document.getElementById("addQuestion").disabled = !importedPreview || Boolean(savedExam);
    document.getElementById("saveDraft").disabled = !importedPreview || Boolean(savedExam) || cards.length === 0;
  }

  function addQuestion(part) {
    if (!importedPreview || savedExam) {
      return;
    }
    var existing = getReviewedQuestions().filter(function (question) {
      return question.part === part;
    });
    var nextNumber = existing.reduce(function (maximum, question) {
      return Math.max(maximum, question.number || 0);
    }, 0) + 1;
    var card = appendQuestionCard(document.getElementById("previewQuestionList"), {
      part: part,
      number: nextNumber,
      questionText: "",
      passage: "",
      optionA: "",
      optionB: "",
      optionC: "",
      optionD: "",
      correctAnswer: "",
      images: [],
    });
    document.getElementById("previewEmptyState").hidden = true;
    updateQuestionSummary();
    var questionField = card.querySelector('[data-question-field="questionText"]');
    questionField.focus();
    card.scrollIntoView({ behavior: "smooth", block: "center" });
  }

  function getReviewedQuestions() {
    return Array.prototype.map.call(
      document.querySelectorAll("[data-imported-question]"),
      function (card) {
        var question = {};
        card.querySelectorAll("[data-question-field]").forEach(function (field) {
          var value = field.value.trim();
          question[field.dataset.questionField] =
            field.dataset.questionField === "part" || field.dataset.questionField === "number"
              ? Number(value)
              : value;
        });
        question.images = card.questionImages || [];
        return question;
      },
    );
  }

  function apiUrl(path) {
    var appPath = window.abp && window.abp.appPath ? window.abp.appPath : "/";
    return appPath.replace(/\/?$/, "/") + path;
  }

  function getAntiForgeryToken() {
    var token = document.querySelector('#examEditorForm input[name="__RequestVerificationToken"]');
    return token ? token.value : "";
  }

  function getApiErrorMessage(body) {
    if (!body) {
      return "";
    }

    if (body.detail || body.title) {
      return body.detail || body.title;
    }

    if (body.error && (body.error.message || body.error.details)) {
      return body.error.message || body.error.details;
    }

    return body.message || "";
  }

  async function readApiResponse(response) {
    var contentType = response.headers.get("content-type") || "";
    var body = null;
    if (contentType.includes("json")) {
      try {
        body = await response.json();
      } catch (error) {
        body = null;
      }
    }

    if (!response.ok) {
      if (response.redirected || response.status === 401) {
        throw new Error("Phiên đăng nhập đã hết hạn. Hãy đăng nhập lại rồi thử lại.");
      }
      if (response.status === 403) {
        throw new Error("Tài khoản hiện tại không có quyền nhập hoặc lưu đề thi.");
      }
      if (response.status === 404) {
        throw new Error("Không tìm thấy API hoặc đề thi. Hãy tải lại trang để dùng phiên bản mới nhất.");
      }
      if (response.status === 413) {
        throw new Error("Tệp tải lên vượt giới hạn 25 MB của chức năng nhập đề.");
      }
      if (response.status === 409) {
        throw new Error(getApiErrorMessage(body) || "Mã đề bị trùng. Hãy đổi sang mã đề khác.");
      }
      if (response.status >= 500) {
        throw new Error(
          (body && body.detail) ||
            "Máy chủ gặp lỗi khi xử lý đề. Hãy kiểm tra migration và log máy chủ trước khi thử lại.",
        );
      }
      throw new Error(
        getApiErrorMessage(body) ||
          (response.status === 400
            ? "Dữ liệu hoặc tệp không hợp lệ. Hãy kiểm tra nội dung DOCX và các trường bắt buộc."
            : "Không thể xử lý yêu cầu (HTTP " + response.status + ")."),
      );
    }

    if (!body) {
      if (response.redirected) {
        throw new Error("Phiên đăng nhập đã hết hạn. Hãy đăng nhập lại rồi thử lại.");
      }
      throw new Error("Máy chủ trả về phản hồi không hợp lệ. Vui lòng thử lại.");
    }
    return body;
  }

  async function uploadDocxForPreview() {
    if (!selectedDocx) {
      showNotice(document.getElementById("importNotice"), "Trước tiên hãy chọn file .docx.", "error");
      return;
    }

    var button = document.getElementById("importDocx");
    var formData = new FormData();
    formData.append("file", selectedDocx, selectedDocx.name);
    formData.append("__RequestVerificationToken", getAntiForgeryToken());
    button.disabled = true;
    document.getElementById("cancelDocx").disabled = true;
    setDocxStatus("Đang tải lên và phân tích...", "docx-status-uploading");
    showNotice(document.getElementById("importNotice"), "Đang gửi tệp lên máy chủ để đọc nội dung Word...", "info");

    try {
      var response = await fetch(apiUrl("api/toeic-exams/import-preview"), {
        method: "POST",
        body: formData,
        credentials: "same-origin",
        headers: { Accept: "application/json" },
      });
      importedPreview = await readApiResponse(response);
      document.getElementById("examTitle").value = importedPreview.suggestedTitle;
      document.getElementById("examCode").value = importedPreview.suggestedTitle
        .normalize("NFKD")
        .replace(/[\u0300-\u036f]/g, "")
        .toUpperCase()
        .replace(/[^A-Z0-9]+/g, "-")
        .replace(/^-|-$/g, "")
        .slice(0, 64);
      renderImportedQuestions(importedPreview);
      savedExam = null;
      setEditorReadOnly(false);
      updateQuestionSummary();
      updatePublishButton();
      setDocxStatus("Đã phân tích · chưa lưu", "docx-status-uploaded");
      showNotice(
        document.getElementById("importNotice"),
        "Đã đọc " + importedPreview.questions.length + " câu từ tài liệu. Hãy kiểm tra/chỉnh sửa nội dung; đề chưa được lưu.",
        "success",
      );
    } catch (error) {
      importedPreview = null;
      document.getElementById("saveDraft").disabled = true;
      setDocxStatus("Không thể nhập", "docx-status-error");
      showNotice(
        document.getElementById("importNotice"),
        error instanceof TypeError
          ? "Không thể kết nối tới máy chủ. Hãy kiểm tra ứng dụng đang chạy rồi thử lại."
          : error.message,
        "error",
      );
    } finally {
      button.disabled = !selectedDocx || Boolean(importedPreview);
      document.getElementById("cancelDocx").disabled = !selectedDocx;
    }
  }

  async function saveImportedDraft() {
    if (!importedPreview || savedExam) {
      return;
    }

    var form = document.getElementById("examEditorForm");
    if (!form.reportValidity()) {
      return;
    }

    if (!window.confirm("Lưu đề này cùng các câu hỏi vào cơ sở dữ liệu dưới dạng bản nháp?")) {
      return;
    }

    var questions = getReviewedQuestions();
    if (questions.length === 0 || questions.length > 300) {
      showNotice(document.getElementById("editorNotice"), "Đề cần có từ 1 đến 300 câu hỏi.", "error");
      return;
    }

    var seenNumbers = new Set();
    var invalidQuestion = questions.find(function (question) {
      var key = question.part + ":" + question.number;
      var duplicate = seenNumbers.has(key);
      seenNumbers.add(key);
      return question.part < 5 || question.part > 7 ||
        question.number < 1 || question.number > 300 || duplicate ||
        !question.questionText || !question.optionA || !question.optionB ||
        !question.optionC || !question.optionD ||
        question.questionText.length > 4000 ||
        (question.passage && question.passage.length > 20000) ||
        question.optionA.length > 2000 || question.optionB.length > 2000 ||
        question.optionC.length > 2000 || question.optionD.length > 2000;
    });
    if (invalidQuestion) {
      showNotice(
        document.getElementById("editorNotice"),
        "Kiểm tra câu hỏi: cần số câu hợp lệ và duy nhất trong từng Part, nội dung, đủ A–D và đúng giới hạn ký tự.",
        "error",
      );
      return;
    }

    var button = document.getElementById("saveDraft");
    button.disabled = true;
    try {
      var response = await fetch(apiUrl("api/toeic-exams"), {
        method: "POST",
        credentials: "same-origin",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
          RequestVerificationToken: getAntiForgeryToken(),
        },
        body: JSON.stringify({
          title: document.getElementById("examTitle").value.trim(),
          code: document.getElementById("examCode").value.trim(),
          durationMinutes: Number(document.getElementById("examDuration").value),
          sourceFileName: importedPreview.sourceFileName,
          questions: questions,
        }),
      });
      savedExam = await readApiResponse(response);
      showNotice(
        document.getElementById("editorNotice"),
        "Đã lưu bản nháp #" + savedExam.id + " (" + savedExam.questionCount + " câu) vào cơ sở dữ liệu.",
        "success",
      );
      button.textContent = "Đã lưu bản nháp";
      document.getElementById("savedExamId").value = savedExam.id;
      setEditorReadOnly(true);
      updatePublishButton();
    } catch (error) {
      button.disabled = false;
      showNotice(
        document.getElementById("editorNotice"),
        error instanceof TypeError
          ? "Không thể kết nối tới máy chủ. Hãy kiểm tra ứng dụng đang chạy rồi thử lại."
          : error.message,
        "error",
      );
    }
  }

  async function loadSavedExam() {
    var examId = Number(document.getElementById("savedExamId").value);
    if (!Number.isSafeInteger(examId) || examId < 1) {
      showNotice(document.getElementById("editorNotice"), "Nhập ID đề thi hợp lệ.", "error");
      return;
    }

    var button = document.getElementById("loadSavedExam");
    button.disabled = true;
    try {
      var response = await fetch(apiUrl("api/toeic-exams/" + examId), {
        method: "GET",
        credentials: "same-origin",
        headers: { Accept: "application/json" },
      });
      var exam = await readApiResponse(response);
      importedPreview = {
        sourceFileName: exam.sourceFileName || "Đề đã lưu",
        suggestedTitle: exam.title,
        questions: exam.questions || [],
      };
      savedExam = exam;
      selectedDocx = null;
      document.getElementById("examFile").value = "";
      document.getElementById("docxDropzone").hidden = false;
      document.getElementById("docxFileRow").hidden = true;
      document.getElementById("importDocx").disabled = true;
      document.getElementById("cancelDocx").disabled = true;
      document.getElementById("examTitle").value = exam.title;
      document.getElementById("examCode").value = exam.code;
      document.getElementById("examDuration").value = exam.durationMinutes;
      renderImportedQuestions(importedPreview);
      setEditorReadOnly(true);
      document.getElementById("saveDraft").textContent = "Đề đã lưu #" + exam.id;
      updatePublishButton();
      showNotice(
        document.getElementById("editorNotice"),
        "Đã tải đề #" + exam.id + " và " + importedPreview.questions.length +
          " câu từ database. Trạng thái: " +
          (exam.status === "Published" ? "đã xuất bản." : "bản nháp, có thể xuất bản."),
        "success",
      );
    } catch (error) {
      showNotice(
        document.getElementById("editorNotice"),
        error instanceof TypeError
          ? "Không thể kết nối tới máy chủ. Hãy kiểm tra ứng dụng đang chạy rồi thử lại."
          : error.message,
        "error",
      );
    } finally {
      button.disabled = false;
    }
  }

  function cancelDocx() {
    selectedDocx = null;
    document.getElementById("examFile").value = "";
    document.getElementById("docxDropzone").hidden = false;
    document.getElementById("docxFileRow").hidden = true;
    document.getElementById("importDocx").disabled = true;
    document.getElementById("cancelDocx").disabled = true;
    document.getElementById("importNotice").classList.remove("visible");
    resetImportedPreview();
    setDocxStatus("Chưa tải lên", "toeic-status-draft");
  }

  async function publishSavedExam() {
    if (!savedExam || savedExam.status !== "Draft") {
      return;
    }

    if (!window.confirm("Xuất bản đề thi này để hoàn tất cấu hình?")) {
      return;
    }

    var button = document.getElementById("publishExam");
    button.disabled = true;
    try {
      var response = await fetch(apiUrl("api/toeic-exams/" + savedExam.id + "/publish"), {
        method: "POST",
        credentials: "same-origin",
        headers: {
          Accept: "application/json",
          RequestVerificationToken: getAntiForgeryToken(),
        },
      });
      savedExam = await readApiResponse(response);
      updatePublishButton();
      showNotice(
        document.getElementById("editorNotice"),
        "Đã xuất bản đề thi #" + savedExam.id + " (" + savedExam.questionCount + " câu).",
        "success",
      );
    } catch (error) {
      button.disabled = false;
      showNotice(
        document.getElementById("editorNotice"),
        error instanceof TypeError
          ? "Không thể kết nối tới máy chủ. Hãy kiểm tra ứng dụng đang chạy rồi thử lại."
          : error.message,
        "error",
      );
    }
  }

  workspace.addEventListener("click", function (event) {
    var target = event.target.closest("button, a");
    if (!target) {
      return;
    }

    if (target.id === "modeSwitch") {
      showMode(workspace.dataset.mode === "student" ? "admin" : "student");
      return;
    }
    if (target.hasAttribute("data-open-editor")) {
      event.preventDefault();
      document.querySelectorAll("[data-admin-screen]").forEach(function (screen) {
        screen.hidden = screen.dataset.adminScreen !== "editor";
      });
      document.getElementById("editorNotice").classList.remove("visible");
      return;
    }
    if (target.hasAttribute("data-back-dashboard")) {
      document.querySelectorAll("[data-admin-screen]").forEach(function (screen) {
        screen.hidden = screen.dataset.adminScreen !== "dashboard";
      });
      return;
    }
    if (target.id === "chooseDocx") {
      document.getElementById("examFile").click();
      return;
    }
    if (target.id === "importDocx") {
      uploadDocxForPreview();
      return;
    }
    if (target.id === "cancelDocx") {
      cancelDocx();
      return;
    }
    if (target.id === "saveDraft") {
      saveImportedDraft();
      return;
    }
    if (target.id === "publishExam") {
      publishSavedExam();
      return;
    }
    if (target.id === "loadSavedExam") {
      loadSavedExam();
      return;
    }
    if (target.id === "addQuestion") {
      addQuestion(5);
      return;
    }
    if (target.hasAttribute("data-add-question-part")) {
      addQuestion(Number(target.dataset.addQuestionPart));
      return;
    }
    if (target.hasAttribute("data-delete-question")) {
      target.closest("[data-imported-question]").remove();
      updateQuestionSummary();
      return;
    }
    if (target.hasAttribute("data-open-exam")) {
      showStudentScreen("detail");
      return;
    }
    if (target.hasAttribute("data-start-exam")) {
      showStudentScreen("exam");
      renderQuestionGrid();
      startTimer();
      return;
    }
    if (target.dataset.studentNav) {
      showStudentScreen(target.dataset.studentNav);
      return;
    }
    if (target.dataset.question) {
      setCurrentQuestion(Number(target.dataset.question));
      return;
    }
    if (target.dataset.questionStep) {
      setCurrentQuestion(currentQuestion + Number(target.dataset.questionStep));
      return;
    }
    if (target.id === "markQuestion") {
      markedQuestions[currentQuestion] = !markedQuestions[currentQuestion];
      setCurrentQuestion(currentQuestion);
      return;
    }
    if (target.id === "showSubmit") {
      document.getElementById("answeredCount").textContent = Object.keys(answers).length;
      document.getElementById("submitPrompt").hidden = false;
      target.hidden = true;
      return;
    }
    if (target.hasAttribute("data-cancel-submit")) {
      document.getElementById("submitPrompt").hidden = true;
      document.getElementById("showSubmit").hidden = false;
      return;
    }
    if (target.hasAttribute("data-confirm-submit")) {
      if (timerHandle) {
        window.clearInterval(timerHandle);
        timerHandle = null;
      }
      showStudentScreen("result");
    }
  });

  document.getElementById("examSearch").addEventListener("input", applyExamFilters);
  document.getElementById("examStatus").addEventListener("change", applyExamFilters);
  document.getElementById("clearExamFilters").addEventListener("click", function () {
    document.getElementById("examSearch").value = "";
    document.getElementById("examStatus").value = "";
    applyExamFilters();
  });

  document.getElementById("examFile").addEventListener("change", function (event) {
    selectDocx(event.target.files[0]);
  });
  document.getElementById("previewQuestionList").addEventListener("input", updateQuestionSummary);
  document.getElementById("previewQuestionList").addEventListener("change", updateQuestionSummary);
  document.getElementById("examDuration").addEventListener("input", updateQuestionSummary);
  document.getElementById("docxDropzone").addEventListener("click", function (event) {
    if (event.target.closest("button")) {
      return;
    }
    document.getElementById("examFile").click();
  });
  document.getElementById("docxDropzone").addEventListener("keydown", function (event) {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      document.getElementById("examFile").click();
    }
  });
  ["dragenter", "dragover"].forEach(function (eventName) {
    document.getElementById("docxDropzone").addEventListener(eventName, function (event) {
      event.preventDefault();
      event.currentTarget.classList.add("drag-over");
    });
  });
  ["dragleave", "drop"].forEach(function (eventName) {
    document.getElementById("docxDropzone").addEventListener(eventName, function (event) {
      event.preventDefault();
      event.currentTarget.classList.remove("drag-over");
    });
  });
  document.getElementById("docxDropzone").addEventListener("drop", function (event) {
    selectDocx(event.dataTransfer.files[0]);
  });

  document.getElementById("examEditorForm").addEventListener("submit", function (event) {
    event.preventDefault();
  });

  document.querySelectorAll('input[name="answer"]').forEach(function (input) {
    input.addEventListener("change", function () {
      answers[currentQuestion] = input.value;
      renderQuestionGrid();
    });
  });

  document.querySelectorAll(".test-part-tab").forEach(function (tab) {
    tab.addEventListener("click", function () {
      document.querySelectorAll(".test-part-tab").forEach(function (item) {
        item.classList.remove("active");
      });
      tab.classList.add("active");
      var part = tab.textContent.trim();
      questionRange =
        part === "Part 5" ? [1, 30] : part === "Part 6" ? [31, 46] : [147, 200];
      setCurrentQuestion(questionRange[0]);
    });
  });

  showMode(window.matchMedia("(max-width: 760px)").matches ? "student" : "admin");
})();
