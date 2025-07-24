const originalFetch = window.fetch;

window.fetch = async function (input, init = {}) {
    const token = localStorage.getItem('jwtToken');

    if (token) {
        init.headers = {
            ...init.headers,
            'Authorization': `Bearer ${token}`
        };
    }

    const response = await originalFetch(input, init);

    // 401 (Unauthorized) o 403 (Forbidden)
    if (response.status === 401 || response.status === 403) {
        console.warn('Unauthorized or Forbidden access. Redirecting to login.');
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('userData');
        window.location.href = '/Account/Login';
    }

    return response;
};